# 🎵 Nexus Music Choice Worker

**Nexus Music Choice Worker** é o núcleo do sistema **Music Choice**, um microserviço desenvolvido para gerenciar experiências musicais colaborativas em tempo real. Ele permite que múltiplos usuários interajam com uma fila musical compartilhada — adicionando, removendo e votando em músicas — enquanto mantém integração contínua com serviços externos como Spotify (atualmente integrado) e outras plataformas de streaming no futuro.

Atualmente o sistema está em fase de desenvolvimento e testes, com integração ativa apenas ao Spotify, mas foi projetado desde o início para suportar múltiplas plataformas (Apple Music, Deezer, Tidal, entre outras). Além disso, no futuro, o sistema permitirá que a música seja tocada em várias caixas de som, com a reprodução local no dispositivo em vez de depender diretamente da plataforma de streaming.

---

## 🚀 Visão Geral

O Nexus Music Choice Worker transforma qualquer dispositivo em um “centro musical coletivo”, onde todos os participantes podem contribuir para a trilha sonora do momento. Ele administra a fila de reprodução, controla votações e garante que as interações sigam regras configuráveis, sempre com foco em manter a experiência equilibrada e fluida.

---

## 🔧 Tecnologias Utilizadas

- **.NET 9** — plataforma robusta e moderna para desenvolvimento backend.
- **Spotify Web API** — integração principal atual.
- **Arquitetura Modular** — preparada para expansão e inclusão de novos serviços.
- **Named Pipes (IPC)** — comunicação rápida e eficiente entre processos no sistema.
- **JSON Protocol** — padrão de troca de mensagens leve e extensível.
- **BackgroundService** — execução contínua e resiliente no backend.
- **Structured Logging** — geração de logs detalhados para rastreabilidade e análise.
- **Configuração Flexível** — ajustes centralizados nos arquivos `appsettings`.

---

## ✨ Principais Funcionalidades

| Funcionalidade                    | Descrição                                                                                     |
|-----------------------------------|------------------------------------------------------------------------------------------------|
| 🎶 **Fila Musical Dinâmica**      | Mantém a playlist ativa, adaptando automaticamente às preferências do grupo.                   |
| 🗳 **Votação para Pular Músicas** | Permite que os participantes votem para pular faixas atuais com base em percentuais definidos. |
| ➕ **Adição de Músicas**          | Usuários podem adicionar músicas à fila, com regras para evitar abusos ou repetições.          |
| ➖ **Remoção Antecipada**         | Participantes podem votar para retirar músicas específicas antes que sejam tocadas.            |
| 🔍 **Filtragem e Categorização** | As músicas passam por filtros automáticos para garantir alinhamento ao gênero/vibe configurados. |
| 🌐 **Alternância de Plataformas**| Sistema preparado para alternar entre diferentes serviços de streaming (futuro planejado).      |
| 📡 **Controle Multi-Som**        | Planejado para permitir que a música toque em várias caixas de som, usando o dispositivo local para reprodução. |
| 📊 **Coleta de Métricas**        | Geração de logs e dados que permitem análises sobre preferências, uso e interações do sistema. |
| 🔒 **Gerenciamento de Conexões** | Suporte a múltiplas conexões simultâneas, garantindo estabilidade e performance.               |
| ⏯ **Interface de Controle Remoto** | Operação remota para gerenciar a fila e comandos por meio dos clientes conectados.            |

---

## 🏗️ Arquitetura do Sistema

A aplicação foi projetada com foco em modularidade e separação de responsabilidades, o que facilita a manutenção e a inclusão de novos recursos.

| Componente                 | Responsabilidade Principal                                                                          |
|----------------------------|-----------------------------------------------------------------------------------------------------|
| **Worker Service**         | Gerencia o ciclo de vida principal, orquestrando todos os componentes.                             |
| **Connection Manager**     | Controla conexões simultâneas com clientes usando Named Pipes.                                      |
| **Command Processor**      | Interpreta comandos recebidos e dispara ações correspondentes.                                      |
| **Interaction Service**    | Contém a lógica principal: gerenciamento de fila, votos, filtros e integração com serviços externos. |
| **Integration Modules**    | Módulos plugáveis para comunicação com APIs de streaming (atualmente Spotify, outros no futuro).    |
| **Multi-Sound Support**    | Funcionalidade planejada para permitir controle de múltiplas caixas de som e reprodução local.     |

📎 **Mais detalhes sobre como as mensagens e eventos IPC são processados estão disponíveis em:**  
[📄 Documentação: Processamento de Mensagens IPC](./Documentation/IPC_CONNECTION.MD)

---

## ⚙️ Configuração

As principais configurações ficam nos arquivos `appsettings.json` e `appsettings.Development.json`. Alguns exemplos importantes:

| Parâmetro                            | Função                                                                                          |
|--------------------------------------|------------------------------------------------------------------------------------------------|
| `MinimumVotePercentage`              | Percentual mínimo de votos necessários para pular uma música (ex.: 60%).                        |
| `MinimumActiveParticipants`          | Número mínimo de usuários ativos para habilitar interações como votações.                      |
| `MaxSongsPerUserPerWindow`           | Limite de músicas que um usuário pode adicionar em um período configurado.                     |
| `SongAdditionWindowMinutes`          | Janela de tempo (em minutos) para controle do limite por usuário.                              |
| `AllowedGenres`                      | Lista de gêneros musicais permitidos.                                                         |
| `ActiveMusicPlatform`                | Plataforma de streaming em uso (Spotify, Apple Music, Deezer, etc.).                           |
| `Logging.Level`                      | Nível de detalhamento dos logs.                                                                |
| `Metrics.Enabled`                    | Ativa/desativa a coleta de métricas em tempo real.                                             |
| `RetryPolicy.MaxRetries`             | Número máximo de tentativas ao lidar com falhas de comunicação com serviços externos.          |
| `MultiSound.Enabled`                 | Ativa/desativa o gerenciamento de múltiplas caixas de som e dispositivos locais.               |

---

## 📊 Logs e Métricas

O sistema gera logs estruturados para:

- 🕵️ **Rastreamento de Ações** — entender o que aconteceu e quando.
- 🎧 **Análise de Preferências** — descobrir padrões e tendências musicais.
- 🛠 **Otimização** — identificar pontos de melhoria no uso do sistema.
- 📡 **Integração com Dashboards** — conexão com ferramentas como Grafana e Kibana para visualização em tempo real.

---

## 🔗 Integração com APIs de Música

Atualmente, o projeto utiliza a **Spotify Web API** para:

- Buscar e gerenciar músicas.
- Controlar playlists e fila de reprodução.
- Consultar informações de faixas.

✅ **Planejamento futuro**: Apple Music, Deezer, Tidal, YouTube Music e outros, com módulos plugáveis seguindo contratos já definidos no sistema.

---

## 🚀 Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/)
- Conta Spotify Premium (com credenciais configuradas na [Spotify Developer Platform](https://developer.spotify.com/))
- Sistema operacional com suporte a Named Pipes (Windows, Linux)
- Acesso e ajuste ao arquivo `appsettings.json`

---

## 📜 Licença

Este projeto está licenciado sob a **MIT License**. Consulte o arquivo `LICENSE` para mais informações.

---

🚀 **Music Choice: escolha coletiva, experiência única.**
