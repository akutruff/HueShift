FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY HueShift/*.csproj ./HueShift/
COPY ArtDotNet/*.csproj ./ArtDotNet/

WORKDIR /app/HueShift
RUN dotnet restore

# copy and publish app and libraries
WORKDIR /app/
COPY HueShift/. ./HueShift/
COPY ArtDotNet/. ./ArtDotNet/
WORKDIR /app/HueShift
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app

RUN mkdir -p config

VOLUME /config

ENV UDPPORT 6454

EXPOSE ${UDPPORT}
EXPOSE ${UDPPORT}/udp

COPY --from=build /app/HueShift/out ./

ENTRYPOINT ["dotnet", "HueShift.dll", "--configuration-file", "/config/hueshift-config.json"]
