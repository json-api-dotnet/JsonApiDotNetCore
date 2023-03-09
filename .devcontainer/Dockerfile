# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.245.2/containers/dotnet/.devcontainer/base.Dockerfile

# [Choice] .NET version: 6.0, 3.1, 6.0-bullseye, 3.1-bullseye, 6.0-focal, 3.1-focal
ARG VARIANT="3.1"
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-${VARIANT}

RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0.4xx --install-dir /usr/share/dotnet

# RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
# RUN unzip awscliv2.zip
# RUN sudo ./aws/install

# RUN apt-get update && \
#     apt-get install --no-install-recommends neovim vim zsh -y
    
WORKDIR /workspaces/discover-server

# RUN mkdir /workspaces/travefy/node_modules && chown vscode:vscode -R /workspaces/travefy

# [Choice] Node.js version: none, lts/*, 18, 16, 14
# ARG NODE_VERSION="18"
# RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

RUN su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install 18" 2>&1

# [Optional] Uncomment this line to install global node packages.
#RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g gulp-cli" 2>&1
#RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g nginx-conf" 2>&1


ARG HOME_DIR="/home/vscode"
RUN su vscode -c 'curl https://raw.githubusercontent.com/git/git/master/contrib/completion/git-completion.bash -o ${HOME_DIR}/.git-completion.bash &&\
    echo "[ -f ${HOME_DIR}/.git-completion.bash ] && source ${HOME_DIR}/.git-completion.bash" >> ${HOME_DIR}/.bashrc'
    
USER vscode
RUN dotnet tool install Octopus.DotNet.Cli --global 
RUN dotnet tool install Swashbuckle.AspNetCore.Cli --global --version 6.5.0
RUN dotnet tool install --global dotnet-ef
ENV PATH="/home/vscode/.dotnet/tools:${PATH}"	
