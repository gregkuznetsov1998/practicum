// var app = require('express')();
// var http = require('http').Server(app);
 
// app.get('/', function(req, res){
//     res.send('OK');
// });
 
// http.listen(3000, function(){
//     console.log('HTTP server started on port 3000');
// });

const axios = require('axios');
var app = require('express')();
var http = require('http').Server(app);

// Конфигурация
const DEVICE_SERVICE_URL = 'http://devices-api:8080';
const TEMPERATURE_API_URL = 'http://temperature-api:8081';

app.get('/api/v1/sensors', function(req, res){
    res.send('OK');
});

// Получение телеметрии устройства
app.get('/api/v1/sensors/:id', async (req, res) => {
    try {
        const deviceId = req.params.id;
        
        // Получение метаданных устройства
        const deviceResponse = await axios.get(`${DEVICE_SERVICE_URL}/api/v1/sensors/${deviceId}`);
        const device = deviceResponse.data;
        
        // Для температурных сенсоров - запрос к внешнему API
        if (device.type === 'Temperature') {
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