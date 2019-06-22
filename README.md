# HiP-ThumbnailService
Microservice (REST API) for generating thumbnails.

## Development Environment Setup
* Clone the repository
* Configure the app:
  Create a file `appsettings.Development.json` in the same folder as `HiP-ThumbnailService.csproj` with the following content, replacing "YOUR URL PREFIX" with a valid URL according to the documentation for [ThumbnailConfig.HostUrl](https://github.com/HiP-App/HiP-ThumbnailService/blob/develop/HiP-ThumbnailService/Utility/ThumbnailConfig.cs):
    ```
    {
        "Thumbnails": {
            "HostUrl": "YOUR URL PREFIX"
        }
    }
    ```
* Launch the app
  * via Visual Studio: Open the solution (*.sln) and run the app (F5)
  * via Terminal: Execute `dotnet run` from the project folder containing `HiP-ThumbnailService.csproj`

The app is preconfigured to run on dev machines with minimal manual configuration. See [appsettings.json](https://github.com/HiP-App/HiP-ThumbnailService/blob/develop/HiP-ThumbnailService/appsettings.json) for a list of configuration fields and their default values.

## Core Concepts
When requesting thumbnails via `GET /api/Thumbnails?Url=...`, the thumbnail service retrieves the original image from another server or service, resizes and crops the image, and then returns it. The URL to the original image is constructed from two parts:

1. **The "HostUrl" configured in the thumbnail service**  
   (example: "https://docker-hip.cs.upb.de/develop/")
1. **The relative URL given by the client via `?Url=`**  
   (example: "datastore/Media/42/File")

Thumbnails of different sizes and formats are cached. When the original image changes, the service owning the image needs to clear the thumbnail cache for that image by calling `DELETE /api/Thumbnails?Url=...`, otherwise the thumbnail service will continue to return the outdated, cached thumbnails.