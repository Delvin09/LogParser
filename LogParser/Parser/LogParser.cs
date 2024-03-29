﻿using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LogParser
{
    internal class LogParser : ILogParser
    {
        private readonly string[] _excludeExtensions = new string[] { ".gif", ".jpg", ".jpeg", ".bmp", ".css", ".js", ".png" };
        private readonly string _filePath;
        private readonly int _bufferSize;
        private readonly ILogger<LogParser> _logger;
        private readonly ILogRepository _logRepository;

        public LogParser(IConfiguration configuration, ILogRepository logRepository, ILogger<LogParser> logger)
        {
            _filePath = configuration["filePath"] ?? throw new ArgumentNullException(nameof(_filePath));
            _bufferSize = !string.IsNullOrEmpty(configuration["bufferSize"])
                    ? int.Parse(configuration["bufferSize"])
                    : 100000;

            if (_bufferSize <= 0)
                throw new ArgumentException($"{nameof(_bufferSize)} can't be equal or less than 0.");

            _logger = logger;
            _logRepository = logRepository;
        }

        public async Task Parse()
        {
            _logger.LogInformation($"Start parsing at {DateTime.Now}");
            var stopWatch = Stopwatch.StartNew();

            using var reader = File.OpenText(_filePath);
            var buffer = new LogRecord[_bufferSize];
            var insertTask = Task.CompletedTask;
            while (!reader.EndOfStream) {

                _logger.LogTrace($"Start parse block. Position: {reader.BaseStream.Position}");
                var count = ParseBlock(reader, buffer);
                _logger.LogTrace($"End parse block. Position: {reader.BaseStream.Position}; count: {count}");

                await insertTask;
                _logger.LogTrace("Add parsing result to database");
                insertTask = _logRepository.AddLogs(buffer.Take(count).ToArray());
            }

            stopWatch.Stop();
            _logger.LogInformation($"End parsing at {DateTime.Now}; Elapsed: {stopWatch.Elapsed}");
        }

        private int ParseBlock(StreamReader reader, LogRecord[] buffer)
        {
            int index = 0;
            for (; !reader.EndOfStream && index < buffer.Length;)
            {
                var line = reader.ReadLine();
                _logger.LogTrace($"Parsing line: `{line}`.");

                var segments = line.Split(new string[] { " - - " }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length != 2)
                {
                    _logger.LogWarning($"Invalid line format: `{line}`. Skipped.");
                    continue;
                }

                var record = new LogRecord();

                if (!TryParseTimeAndRoutePart(segments[1], record))
                {
                    _logger.LogTrace($"Skipped line: `{line}`. Wrong format.");
                    continue;
                }

                record.Host = segments[0];
                buffer[index++] = record;
            }

            return index + 1;
        }

        private bool TryParseTimeAndRoutePart(ReadOnlySpan<char> segment, LogRecord record)
        {
            var timeSegment = ParseTimePart(segment, out int start);
            if (start >= 0 && start + 1 < segment.Length)
                segment = segment.Slice(start + 1);
            else
                return false;

            if (!ParseRequestResponsePart(segment, record))
                return false;

            record.RequestDateTime = DateTime.ParseExact(timeSegment, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture);
            return true;
        }

        private ReadOnlySpan<char> ParseTimePart(ReadOnlySpan<char> segment, out int end)
        {
            end = -1;

            var start = segment.IndexOf('[');
            if (start >= 0 && start + 1 < segment.Length)
                segment = segment.Slice(start + 1);
            else
                return ReadOnlySpan<char>.Empty;

            end = segment.IndexOf(']');
            if (end > 0 && end <= segment.Length)
                segment = segment.Slice(0, end);
            else
                return ReadOnlySpan<char>.Empty;

            return segment;
        }

        private bool ParseRequestResponsePart(ReadOnlySpan<char> linePart, LogRecord record)
        {
            int start = linePart.IndexOf('"');
            int end = linePart.LastIndexOf('"');

            if (start < 0 || end < 0 || end <= start)
            {
                _logger.LogError($"Can't parse line {linePart.ToString()} - didn't find Request segment (`\"`)");
                return false;
            }

            if (!ParseRoute(linePart.Slice(start + 1, end - start - 1), record))
                return false;

            ParseResponse(linePart.Slice(end + 2), record);
            return true;
        }

        private void ParseResponse(ReadOnlySpan<char> span, LogRecord record)
        {
            var delimeter = span.IndexOf(' ');
            record.ResultCode = int.Parse(span.Slice(0, delimeter));

            int.TryParse(span.Slice(delimeter), out int size);
            record.ResponseSize = size;
        }

        private bool ParseRoute(ReadOnlySpan<char> span, LogRecord record)
        {
            var start = span.IndexOf(" ", StringComparison.Ordinal) + 1;
            var end = span.LastIndexOf("HTTP", StringComparison.OrdinalIgnoreCase);
            var segments = end < 0 ? span.Slice(start) : span.Slice(start, end - start - 1);

            var lastSegment = GetLastRouteSegment(segments);
            if (!lastSegment.IsEmpty)
            {
                var queryStart = lastSegment.IndexOf('?');
                ReadOnlySpan<char> extension, queryParams = ReadOnlySpan<char>.Empty;
                if (queryStart >= 0)
                {
                    segments = segments.Slice(0, queryStart);
                    extension = Path.GetExtension(lastSegment.Slice(0, queryStart));
                    queryParams = lastSegment.Slice(queryStart);
                }
                else
                {
                    extension = Path.GetExtension(lastSegment);
                }

                extension = extension.Trim();

                foreach (var ext in _excludeExtensions)
                {
                    if (extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogTrace($"Exclude line. Extension isn't support.");
                        return false;
                    }
                }

                record.QueryParameters = queryParams.ToString();
                record.Route = segments.ToString();
            }
            return true;
        }

        private ReadOnlySpan<char> GetLastRouteSegment(ReadOnlySpan<char> segments)
        {
            var start = segments.LastIndexOf('/');
            return start < segments.Length - 1 ? segments.Slice(start + 1) : ReadOnlySpan<char>.Empty;
        }
    }
}
