FROM microsoft/dotnet:3.1-sdk AS build
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

FROM microsoft/dotnet:3.1-runtime AS runtime
WORKDIR /app

RUN mkdir -p config

VOLUME /config

ENV UDPPORT 6454

EXPOSE ${UDPPORT}
EXPOSE ${UDPPORT}/udp

COPY --from=build /app/HueShift/out ./

ENTRYPOINT ["dotnet", "HueShift.dll", "--configuration-file", "/config/hueshift-config.json"]
