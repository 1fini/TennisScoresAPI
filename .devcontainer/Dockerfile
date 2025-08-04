# Utiliser une image de base avec .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

# Installer .NET 9
RUN apt-get update && \
    apt-get install -y wget && \
    wget https://dotnetcli.blob.core.windows.net/dotnet/9.0.0/dotnet-sdk-9.0.0-linux-x64.tar.gz && \
    tar -zxvf dotnet-sdk-9.0.0-linux-x64.tar.gz -C /usr/share/dotnet && \
    rm dotnet-sdk-9.0.0-linux-x64.tar.gz && \
    ln -s /usr/share/dotnet/sdk/9.0.0 /usr/share/dotnet/sdk && \
    ln -s /usr/share/dotnet/shared/Microsoft.AspNetCore.App/9.0.0 /usr/share/dotnet/shared/Microsoft.AspNetCore.App && \
    ln -s /usr/share/dotnet/shared/Microsoft.NETCore.App/9.0.0 /usr/share/dotnet/shared/Microsoft.NETCore.App

# Vérifier l'installation de .NET 9
RUN dotnet --version

# Copier le projet dans le conteneur
COPY . /workspace

# Définir le répertoire de travail
WORKDIR /workspace

# Exécuter la commande de build
CMD ["dotnet", "build"]
