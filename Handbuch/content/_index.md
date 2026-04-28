---
title: "MeinCMS Handbuch"
date: 2026-04-28
---

# MeinCMS Handbuch

Willkommen im Einrichtungshandbuch für **MeinCMS** – ein mandantenfähiges Wiki/CMS auf Basis von ASP.NET Core 10 und PostgreSQL.

## Inhalt

- [Entwicklungsrechner einrichten](/entwicklung) – Lokale Entwicklungsumgebung aufsetzen
- [Produktionsserver einrichten](/produktion) – Deployment auf Ubuntu Server 24.04 mit Nginx und systemd

## Systemübersicht

| Komponente | Technologie |
|---|---|
| Webanwendung | ASP.NET Core 10 MVC |
| Datenbank | PostgreSQL (via Npgsql + EF Core) |
| Webserver | Nginx (Reverse Proxy via Unix Domain Socket) |
| Prozessmanager | systemd |
| SSL | Let's Encrypt (Certbot) |
| Laufzeitumgebung | .NET 10 Runtime |

## Mandanten

Das System betreibt zwei Mandanten auf einer Instanz:

| Domain | Mandanten-ID |
|---|---|
| `ahrensburg.city` | `main` |
| `doc.ahrensburg.city` | `doc` |
