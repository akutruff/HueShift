FROM microsoft/dotnet:2.2-sdk AS build
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


#COPY dotnetapp/*.csproj ./dotnetapp/
#COPY utils/*.csproj ./utils/
#WORKDIR /app/dotnetapp
#RUN dotnet restore

## copy and publish app and libraries
#WORKDIR /app/
#COPY dotnetapp/. ./dotnetapp/
#COPY utils/. ./utils/
#WORKDIR /app/dotnetapp
#RUN dotnet publish -c Release -o out


## test application -- see: dotnet-docker-unit-testing.md
#FROM build AS testrunner
#WORKDIR /app/tests
#COPY tests/. .
#ENTRYPOINT ["dotnet", "test", "--logger:trx"]


FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app

RUN mkdir -p config

VOLUME /config

ENV UDPPORT 6454

EXPOSE ${UDPPORT}
EXPOSE ${UDPPORT}/udp

COPY --from=build /app/HueShift/out ./


ENTRYPOINT ["dotnet", "HueShift.dll", "--configuration-file", "/config/hueshift-config.json"]
