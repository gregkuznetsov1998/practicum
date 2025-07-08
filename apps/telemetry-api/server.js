const axios = require('axios');
var app = require('express')();
var http = require('http').Server(app);

// Конфигурация
const DEVICE_SERVICE_URL = 'http://devices-api:8080';
const TEMPERATURE_API_URL = 'http://temperature-api:8081';

// Получение телеметрии всех устройств
app.get('/api/v1/sensors', async (req, res) => {
    try {
        // Получаем все устройства
        const devicesResponse = await axios.get(`${DEVICE_SERVICE_URL}/api/v1/sensors`);
        const devices = devicesResponse.data;
        
        // Собираем запросы для температурных сенсоров
        const tempRequests = devices
            .filter(device => device.type === 'temperature')
            .map(device => 
                axios.get(`${TEMPERATURE_API_URL}/temperature?location=${device.location}`)
                    .then(response => ({
                        id: device.id,
                        value: response.data.value,
                        lastUpdated: response.data.timestamp
                    }))
                    .catch(() => null) // Игнорируем ошибки для отдельных сенсоров
            );

        // Параллельно выполняем все запросы
        const tempData = await Promise.all(tempRequests);
        const tempMap = new Map(
            tempData.filter(data => data !== null)
                   .map(data => [data.id, data])
        );

        // Обогащаем данные устройств
        const enrichedDevices = devices.map(device => {
            if (device.type === 'temperature' && tempMap.has(device.id)) {
                const tempInfo = tempMap.get(device.id);
                return {
                    ...device,
                    value: tempInfo.value,
                    lastUpdated: tempInfo.lastUpdated
                };
            }
            return device;
        });

        res.json(enrichedDevices);
    } catch (error) {
        console.error('Failed to fetch devices:', error.message);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Получение телеметрии устройства
app.get('/api/v1/sensors/:id', async (req, res) => {
    try {
        const deviceId = req.params.id;
        
        // Получение метаданных устройства
        const deviceResponse = await axios.get(`${DEVICE_SERVICE_URL}/api/v1/sensors/${deviceId}`);
        const device = deviceResponse.data;
        
        // Для температурных сенсоров - запрос к внешнему API
        if (device.type === 'temperature') {
            const tempResponse = await axios.get(`${TEMPERATURE_API_URL}/temperature?location=${device.location}`);
            return res.json({
                ...device,
                value: tempResponse.data.value,
                lastUpdated: tempResponse.data.timestamp
            });
        }
        
        // Для других типов - возвращаем базовую информацию
        res.json(device);
    } catch (error) {
        res.status(500).json({ error: error });
    }
});

// Обновление значения сенсора
app.patch('/api/v1/sensors/:id/value', async (req, res) => {
    // Логика обновления значения
    res.status(200).json({ message: 'Value updated' });
});

// Получение температуры по местоположению
app.get('/api/v1/sensors/temperature/:location', async (req, res) => {
    try {
        const response = await axios.get(`${TEMPERATURE_API_URL}/temperature?location=${req.params.location}`);
        res.json(response.data);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

http.listen(3000, function(){
    console.log('HTTP server started on port 3000');
});