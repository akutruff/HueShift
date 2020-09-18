# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.140.1/containers/dotnetcore/.devcontainer/base.Dockerfile


# [Choice] .NET Core version: 3.1, 2.1
ARG VARIANT="3.1"
FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-${VARIANT}

# [Option] Install Node.js
ARG INSTALL_NODE="true"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "source /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Option] Install Azure CLI
ARG INSTALL_AZURE_CLI="false"
COPY library-scripts/azcli-debian.sh /tmp/library-scripts/
RUN if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts 

ARG TZ="America/New_York"

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends tzdata \
    && echo ${TZ} > /etc/timezone \    
    && rm /etc/localtime \
    && ln -snf /usr/share/zoneinfo/${TZ} /etc/localtime \
    && dpkg-reconfigure -f noninteractive tzdata \
    && apt-get clean
    