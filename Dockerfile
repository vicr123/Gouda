FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine

ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    tzdata \
    fontconfig \
    font-noto
