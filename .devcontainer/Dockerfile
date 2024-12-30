# Base images:
# - <https://github.com/devcontainers/images/blob/633d1fb3ab2002894d7ee00781863a37c3531bab/src/dotnet/.devcontainer/Dockerfile>
# - <https://github.com/dotnet/dotnet-docker/tree/019198c5f4ee65d20dda7bbf9b6b38980c38a3b1/src/sdk/8.0/bookworm-slim>

FROM mcr.microsoft.com/devcontainers/dotnet:8.0-bookworm-slim



WORKDIR /workspaces/discover-server


RUN su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install 18" 2>&1



ARG HOME_DIR="/home/vscode"
RUN su vscode -c 'curl https://raw.githubusercontent.com/git/git/master/contrib/completion/git-completion.bash -o ${HOME_DIR}/.git-completion.bash &&\
    echo "[ -f ${HOME_DIR}/.git-completion.bash ] && source ${HOME_DIR}/.git-completion.bash" >> ${HOME_DIR}/.bashrc'
    
USER vscode
ENV PATH="/home/vscode/.dotnet/tools:${PATH}"	
