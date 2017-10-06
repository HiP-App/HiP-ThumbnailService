FROM microsoft/dotnet:2.0.0-sdk-jessie

RUN apt-get update && apt-get install make gcc -y

RUN wget https://www.openssl.org/source/openssl-1.1.0f.tar.gz -q && tar xzf openssl-1.1.0f.tar.gz && cd openssl-1.1.0f && ./config && make && make install

RUN mkdir -p /dotnetapp

COPY . /dotnetapp
WORKDIR /dotnetapp/HiP-ThumbnailService

EXPOSE 5000

RUN dotnet restore --no-cache
RUN chmod +x /dotnetapp/HiP-ThumbnailService/run.sh

CMD /dotnetapp/HiP-ThumbnailService/run.sh