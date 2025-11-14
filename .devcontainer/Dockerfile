FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm
ARG TZ
ENV TZ="America/New_York"

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends tzdata \
    && echo ${TZ} > /etc/timezone \    
    && rm /etc/localtime \
    && ln -snf /usr/share/zoneinfo/${TZ} /etc/localtime \
    && dpkg-reconfigure -f noninteractive tzdata \
    && apt-get clean