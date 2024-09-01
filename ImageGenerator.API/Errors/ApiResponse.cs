﻿namespace ImageGenerator.API.Errors
{
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
           Message = message ?? GetMessage(statusCode);
        }
        private string? GetMessage(int statusCode) 
        {
            return statusCode switch
            {
                200 => "Success",
                400 => "Bad Request",
                401 => "You are not Authorized",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => null
            };
        }
    }
}
