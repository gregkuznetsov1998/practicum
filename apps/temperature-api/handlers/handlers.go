package handlers

import (
	"math/rand/v2"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
)

type TemperatureResponse struct {
	Value       float64   `json:"value"`
	Unit        string    `json:"unit"`
	Timestamp   time.Time `json:"timestamp"`
	Location    string    `json:"location"`
	Status      string    `json:"status"`
	SensorID    string    `json:"sensor_id"`
	SensorType  string    `json:"sensor_type"`
	Description string    `json:"description"`
}

func RegisterRoutes(router *gin.RouterGroup) {
	router.Handle("GET", "/temperature", GetTemperature)
	// sensors := router.Group("/")
	// {
	// 	sensors.GET("temperature", GetTemperature)
	// }
}

func GetTemperature(c *gin.Context) {
	location := c.Param("location")
	sensorID := c.Param("sensorID")

	if location == "" {
		switch sensorID {
		case "1":
			location = "Living Room"
		case "2":
			location = "Bedroom"
		case "3":
			location = "Kitchen"
		default:
			location = "Unknown"
		}
	}

	if sensorID == "" {
		switch location {
		case "Living Room":
			sensorID = "1"
		case "Bedroom":
			sensorID = "2"
		case "Kitchen":
			sensorID = "3"
		default:
			sensorID = "0"
		}
	}

	resp := TemperatureResponse{
		Value:       getRand(),
		Unit:        "celsius",
		Timestamp:   time.Now(),
		Location:    location,
		Status:      "active",
		SensorID:    sensorID,
		SensorType:  "temperature",
		Description: "some description",
	}

	c.JSON(http.StatusOK, resp)
}

func getRand() float64 {
	min := -30.0
	max := 40.0

	return min + rand.Float64()*(max-min)
}
