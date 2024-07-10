# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.245.2/containers/dotnet/.devcontainer/base.Dockerfile

# [Choice] .NET version: 6.0, 3.1, 6.0-bullseye, 3.1-bullseye, 6.0-focal, 3.1-focal
ARG VARIANT="6.0"
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-${VARIANT}



WORKDIR /workspaces/discover-server


RUN su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install 18" 2>&1



ARG HOME_DIR="/home/vscode"
RUN su vscode -c 'curl https://raw.githubusercontent.com/git/git/master/contrib/completion/git-completion.bash -o ${HOME_DIR}/.git-completion.bash &&\
    echo "[ -f ${HOME_DIR}/.git-completion.bash ] && source ${HOME_DIR}/.git-completion.bash" >> ${HOME_DIR}/.bashrc'
    
USER vscode
ENV PATH="/home/vscode/.dotnet/tools:${PATH}"	
