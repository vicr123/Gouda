FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
RUN apk add icu tzdata fontconfig font-noto
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
