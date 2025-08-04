# Utiliser une image de base avec .NET 8
FROM mcr.microsoft.com/devcontainers/dotnet:0-8.0

# Installer .NET 9
RUN wget https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.100-preview.7.24407.12/dotnet-sdk-9.0.100-preview.7.24407.12-linux-x64.tar.gz -O dotnet9.tar.gz \
&& mkdir -p /usr/share/dotnet9 \
&& tar -zxf dotnet9.tar.gz -C /usr/share/dotnet9 \
&& rm dotnet9.tar.gz

# Vérifier l'installation de .NET 9
RUN dotnet --version

# Add SDK .NET9 to PATH
ENV DOTNET_ROOT=/usr/share/dotnet9
ENV PATH="${DOTNET_ROOT}:${PATH}"
