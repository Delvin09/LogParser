# Описание

Решение для парсинга файла логов веб сервера (ftp://ita.ee.lbl.gov/traces/NASA_access_log_Jul95.gz), с определением [геолокации](https://api.ipgeolocationapi.com/geolocate/), с сохранением в базе данных и с доступом через WebApi.

Это тестовая задача для прохождения собеседования.

### Настройки

Перед запуском нужно:
   - запустить скрипт `CREATE_DB.sql`, для инициализации базы и таблиц.
   - обновить конекшен стринг `LogDatabase` в `appsettings.json` для парсера и webapi.
   - разархивировать `access_log_Jul95.zip`.
   - в `appsettings.json` указать путь к файлу лога в свойстве `filePath`.
    
 Остальные настройки:
   - `ipLookupBusSize`: размер буфера одновременных запросов в сервис определение геолокиции.
   - `bufferSize`: буфер памяти для парсера.
   - `ParseOn`: включение/выключение парсера.
   - `IpLookupOn`: включение/выключение определения геолокации.
