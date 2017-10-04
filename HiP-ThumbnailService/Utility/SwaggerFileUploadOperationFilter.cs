﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService.Utility
{
    public class SwaggerFileUploadOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation,OperationFilterContext context)
        {
            var fileParam = context.ApiDescription.ParameterDescriptions.FirstOrDefault(x => x.ModelMetadata.ModelType == typeof(IFormFile));

            if (fileParam != null)
            {
                //Deleting all file query parameters (ContentType,ContentDisposition,Headers,Length,Name,FileName)
                List<string> parameterNameList = context.ApiDescription.ParameterDescriptions.ToList().FindAll(x => x.ModelMetadata.ModelType == typeof(IFormFile)).Select(x => x.Name).ToList();
                var operationList = operation.Parameters.ToList();
                operationList.RemoveAll(x => parameterNameList.Any(y => x.Name == y));

                operation.Parameters = operationList;

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "File",
                    In = "formData",
                    Description = "Upload file",
                    Required = true,
                    Type = "file"
                });

                operation.Consumes.Add("application/form-data");
            }
        }
    }
}