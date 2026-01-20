/src
  /0-Common               <-- Version-agnostic extensions, constants, and global utils
  /1-Core
    /Domain               <-- Entities, Exceptions, Repository Interfaces
    /Application
      /Common             <-- Shared DTOs or Behaviors
      /Features
        /Orders
          /v1             <-- GetOrderQueryV1, OrderDtoV1
          /v2             <-- GetOrderQueryV2, OrderDtoV2
  /2-Infrastructure
    /Persistence          <-- EF Core, Migrations, Repositories
    /ExternalClients      <-- API Clients, Message Broker implementations
  /3-Hosts (Presentation)
    /Public.Api
      /Controllers/v1     <-- Routes to Application.v1
      /Controllers/v2     <-- Routes to Application.v2
    /Admin.Api
    /Workers
      /IdentityWorker     <-- Consumer for Auth events
      /ReportWorker       <-- Cron jobs
/deploy
  /nginx
    - nginx.conf          <-- Reverse proxy & routing rules
  /docker
    - docker-compose.yml
/tests
  /Unit
  /Integration