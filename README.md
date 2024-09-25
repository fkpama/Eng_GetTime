# Eng_GetTime

## Protocol de transfert

 Protocole specifique ou Json (configurable en ligne de commande).


## Architecture du code

### Back
 
 Le back est une application console (hors Asp.net pour performance) qui met a disposition un serveur WebSocket sur le port 8181 sur localhost [par default](https://github.com/fkpama/Eng_GetTime/blob/main/Back/Program.cs#L135) (Configurable en ligne de commande, a implementer)
 
 compose de 3 composants principaux:
 
 - [Une classe generant les prix](https://github.com/fkpama/Eng_GetTime/blob/main/Back/PriceGenerator.cs)
    Pour des raisons de performance, cette classe met aussi en cache la version encodee des prix

 - [Une classe](https://github.com/fkpama/Eng_GetTime/blob/main/Back/PriceDispatcher.cs) qui dispatch les mises a jour des prix aux clients en temps reel

 - [Un mechanime d'ordonnancement des updates](https://github.com/fkpama/Eng_GetTime/blob/main/Back/Program.cs#L107)

 ### Front

Le front se connnecte sur le Back sur le port [localhost 8181](https://github.com/fkpama/Eng_GetTime/blob/main/Front/src/environments/environment.ts#L5) par default, l'affichage des donnees se faisant par [l'AppComponent](https://github.com/fkpama/Eng_GetTime/blob/main/Front/src/components/app/app.component.html#L5)

L'application utilise Angular 18, et sa nouvelle API de signalement. Aucune authentification implementee pour le moment.

[Voir documentation](./doc/front.md)

## Environnement de developpement

Le code a ete ecrit (et donc teste) sur [VSCode web](https://orange-space-eureka-vgwjj5gqjv5fpxp4.github.dev/). Le projet du back est donc sous [multiple TargetFrameworks](https://github.com/fkpama/Eng_GetTime/blob/main/Back/GetTime.csproj#L5) (.net5 et .net4.8).

[Voir comment lancer la solution](./doc/debug.md)


 ## TODO

 - Documentation
 - Modification configuration
 - Parsing de la ligne de commande
 - Tests
 - git rebase