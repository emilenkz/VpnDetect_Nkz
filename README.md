# VpnDetect

## Description
**VpnDetect_Nkz** est un plugin qui détecte et bannit si activé les adresses IP étrangères ne provenant pas des pays spécifiquement autorisés dans la `config`.

## Fonctionnalités

- Préviens les administrateurs de la connexion d'un joueur avec un VPN dans le chat du jeu ainsi que le pays d'origine de l'IP du joueur.
- Bannit automatiquement les joueurs localisés hors des pays autorisés dans la config.
- Envoie une notification via un webhook Discord avec des détails sur le joueur et l'IP.
- Possibilité de choisir si le bannissement est activé ou non via la configuration.
- Possibilité de définir les pays autorisés via la configuration.

## Installation

1. Téléchargez le fichier `VpnDetect.dll` depuis la page des releases de ce dépôt.
2. Ajoutez le fichier `VpnDetect.dll` dans le dossier des plugins de votre serveur Novalife.
3. Lancer votre serveur puis quand le fichier `config` est crée, modifier le selon vos besoins.

## Configuration

Le fichier `config.json` vous permet de personnaliser les paramètres du plugin :

- `Webhook`: L'URL du webhook Discord pour recevoir des notifications.
- `AutoBan`: Définir sur `true` pour bannir automatiquement les joueurs venant d'un pays non autorisé, `false` pour désactiver le bannissement automatique.
- `ExcludedCountries`: Liste des codes de pays (ISO 3166-1 alpha-2) autorisés. Par défaut, ce sont "FR" (France), "BE" (Belgique), "CA" (Canada) et "CH" (Suisse).

## Exemple de `config.json`

```json
{
  "Webhook": "https://discord.com/api/webhooks/1234567890/abcdefghijklmnopqrstuvwxyz",
  "AutoBan": false,
  "ExcludedCountries": ["FR", "BE", "CA", "CH", "NL"]
}
```

## Développeurs

Plugin initialement développé par Zari puis repris par emile.cvl car le développeur original a arrêté son développement.

Si vous rencontrez des problèmes ou avez besoin d'aide avec le plugin, vous pouvez me contacter sur Discord : emile.cvl

**Je vous prie de respecter le temps que nous avons investi dans ce plugin. Merci de ne pas l'utiliser de manière inappropriée et de ne pas en altérer l'intégrité.**
