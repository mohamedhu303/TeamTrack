# المرحلة الأولى: تشغيل التطبيق
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# المرحلة الثانية: بناء الكود
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# نسخ ملف المشروع واسترجاع الباكدجات
COPY ["TeamTrack.csproj", "./"]
RUN dotnet restore "TeamTrack.csproj"

# نسخ باقي الملفات
COPY . .

# بناء ونشر الكود
RUN dotnet publish "TeamTrack.csproj" -c Release -o /app/publish

# المرحلة الثالثة: التشغيل النهائي
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TeamTrack.dll"]
