version: '3.5'

services:
  blazorboilerplate:
    image: ${docker_blazorboilerplate_image}
    ports:
      - 80:80
      - 443:443
    depends_on:
      - sqlserver
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT} #Consider changing this in Production
      - Serilog__MinimumLevel__Default=${Serilog__MinimumLevel__Default} #Consider changing this in Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=blazor_boilerplate;Trusted_Connection=True;MultipleActiveResultSets=true;User=sa;Password=${sa_password};Integrated Security=false
      - BlazorBoilerplate__UseSqlServer=true
      - BlazorBoilerplate__ApplicationUrl=blazorboilerplate
      - BlazorBoilerplate__IS4ApplicationUrl=blazorboilerplate
      - BlazorBoilerplate__CertificatePassword=${cert_password}
      - ASPNETCORE_URLS=https://+:443;http://+80
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${cert_password}
      - ASPNETCORE_Kestrel__Certificates__Default__Path=aspnetapp.pfx
    networks:
      - bb
    restart: always #starts on docker service start and restarts on failure

  sqlserver:
    image: ${docker_sqlserver_image}
    volumes:
      - dbdata:/var/opt/mssql
    environment:
      - SA_PASSWORD=${sa_password}
      - ACCEPT_EULA=Y
    ports:
      - 1533:1433 #expose port, so can connect to it using host: 'localhost,1533' | user: sa, password: ${sa_password}
    networks:
      - bb
    restart: always #starts on docker service start and restarts on failure

volumes:
  dbdata:

networks:
  bb:
    name: bb_network
    ipam:
      driver: default
