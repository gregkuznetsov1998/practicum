package handlers

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

func RegisterSensorRoutes(router *gin.RouterGroup) {
	sensors := router.Group("/sensors")
	{
		sensors.GET("", forwardToDeviceService)
		sensors.GET("/:id", forwardToDeviceService)
		sensors.POST("", forwardToDeviceService)
		sensors.PUT("/:id", forwardToDeviceService)
		sensors.DELETE("/:id", forwardToDeviceService)

		sensors.PATCH("/:id/value", forwardToTelemetryService)
		sensors.GET("/temperature/:location", forwardToTelemetryService)
	}
}

func forwardToDeviceService(c *gin.Context) {
	// Реализация перенаправления в C# микросервис
	c.Redirect(http.StatusTemporaryRedirect, "http://localhost:8082"+c.Request.URL.Path)
}

func forwardToTelemetryService(c *gin.Context) {
	// Реализация перенаправления в Node.js микросервис
	c.Redirect(http.StatusTemporaryRedirect, "http://localhost:8083"+c.Request.URL.Path)
}
