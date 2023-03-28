FROM mcr.microsoft.com/dotnet/sdk:7.0.202-bullseye-slim as build
WORKDIR /src

COPY . .
WORKDIR /src/TheGodfather
RUN set -xe; \
    dotnet --version; \
    dotnet publish -c Release -o /app; \
    rm -Rf runtimes/win* runtimes/osx* runtimes/linux-arm* runtimes/linux-mips*; \
    find /app -type f -exec chmod -x {} \; ;\
    chmod +x /app/TheGodfather


FROM mcr.microsoft.com/dotnet/sdk:7.0.202-bullseye-slim
WORKDIR /app

RUN set -xe; \
    useradd -m gf; \
    apt-get update; \
    apt-get install -y curl sudo libsqlite3-0 #libopus0 libsodium23 ffmpeg 

COPY --from=build /app ./
COPY docker-entrypoint.sh /usr/local/sbin
RUN set -xe; \
    chmod +x /usr/local/sbin/docker-entrypoint.sh; \
    chown gf /usr/local/sbin/docker-entrypoint.sh

VOLUME [ "/app/data" ]
ENTRYPOINT [ "/usr/local/sbin/docker-entrypoint.sh" ]
CMD dotnet TheGodfather.dll
