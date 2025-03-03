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
    font-noto \
    font-noto-cjk \
    font-noto-extra \
    font-noto-arabic \
    font-noto-armenian \
    font-noto-cherokee \
    font-noto-devanagari \
    font-noto-ethiopic \
    font-noto-georgian \
    font-noto-hebrew \
    font-noto-lao \
    font-noto-malayalam \
    font-noto-tamil \
    font-noto-thaana \
    font-noto-thai
