# Systems Index: Covenant of Mages

> **Status**: Approved
> **Created**: 2026-08-26
> **Last Updated**: 2026-08-26
> **Source Concept**: design/gdd/game-concept.md
> **Replaces**: Hollow Vow systems index (fully invalid — different genre, single-player)

---

## Overview

Covenant of Mages — кооперативный FPS-dungeon crawler для 4-5 игроков, где победа
строится на реальных стихийных взаимодействиях между магами в реальном времени.
Системы игры делятся на два принципиальных блока: **инфраструктура сессии** (сеть,
лобби, сохранения — фундамент, без которого кооп не существует) и **боевая механика**
(стихийные статусы, кастование, синергии — ядро уникальности).

Главный технический риск проекта: синхронизация стихийных состояний (Wet/Burning/Frozen/
Electrified) между 4-5 игроками в реальном времени. Networking Foundation и Elemental
Status System — bottleneck-системы, которые блокируют всё остальное и требуют ранней
архитектурной фиксации. ADR-0001 (Netcode for GameObjects) написан и переведён в
статус **Accepted** (2026-08-26) — программирование по этой архитектуре разрешено.

Критическое дизайн-требование из прототипа (2026-08-26): система Target Feedback
обязательна для MVP — без видимого индикатора текущей цели синергии ломаются незаметно
для игрока. Это не полировка, а механическая необходимость.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Input System | Core | MVP | Designed (retrofitted 2026-08-26) | [input-system.md](input-system.md) | — |
| 2 | Camera System | Core | MVP | Designed (pending review, 2026-08-26) | [camera-system.md](camera-system.md) | Input System |
| 3 | Health & Damage System | Gameplay | MVP | Designed (pending review, 2026-08-26) | [health-damage-system.md](health-damage-system.md) | — |
| 4 | Networking Foundation | Networking | MVP | Not Started (ADR-0001 Accepted) | — | — |
| 5 | Pixelation Rendering Pipeline | Core | MVP | Not Started | — | — |
| 6 | Audio System | Audio | MVP | Not Started | — | — |
| 7 | Save/Load & Persistence | Persistence | MVP | Not Started | — | — |
| 8 | Player Controller (inferred) | Core | MVP | Not Started | — | Input System, Camera System, Networking Foundation |
| 9 | Elemental Status System | Gameplay | MVP | Not Started | — | Health & Damage System, Networking Foundation |
| 10 | Spell Casting System | Gameplay | MVP | Not Started | — | Input System, Camera System, Elemental Status System, Networking Foundation |
| 11 | Enemy AI System (inferred) | Gameplay | MVP | Not Started | — | Health & Damage System, Networking Foundation |
| 12 | Lobby/Session System (inferred) | Networking | MVP | Not Started | — | Networking Foundation |
| 13 | Character/School Selection (inferred) | Gameplay | MVP | Not Started | — | Lobby/Session System |
| 14 | Elemental Synergy System | Gameplay | MVP | Not Started | — | Elemental Status System, Spell Casting System, Health & Damage System |
| 15 | Target Feedback System (inferred) | UI | MVP | Not Started | — | Input System, Camera System, Spell Casting System |
| 16 | Loadout/Spell Slot System | Gameplay | MVP | Not Started | — | Spell Casting System, Character/School Selection, Save/Load & Persistence |
| 17 | Spectator/Death System | Gameplay | MVP | Not Started | — | Health & Damage System, Networking Foundation, Player Controller |
| 18 | Checkpoint System (inferred) | Gameplay | MVP | Not Started | — | Save/Load & Persistence, Health & Damage System |
| 19 | VFX & Spell Effects System (inferred) | Presentation | MVP | Not Started | — | Spell Casting System, Elemental Status System, Pixelation Rendering Pipeline |
| 20 | Dungeon Structure System | Gameplay | MVP | Not Started | — | Enemy AI System, Player Controller, Checkpoint System |
| 21 | Boss System | Gameplay | MVP | Not Started | — | Enemy AI System, Dungeon Structure System |
| 22 | Puzzle Room System | Gameplay | MVP | Not Started | — | Player Controller, Dungeon Structure System |
| 23 | Loot & Economy System | Economy | MVP | Not Started | — | Dungeon Structure System, Save/Load & Persistence |
| 24 | Manuscript/Spell Unlock System | Progression | MVP | Not Started | — | Loot & Economy System, Loadout/Spell Slot System |
| 25 | Character Progression/Level System | Progression | MVP (minimal) | Not Started | — | Save/Load & Persistence, Health & Damage System |
| 26 | Combat HUD (inferred) | UI | MVP | Not Started | — | Spell Casting System, Health & Damage System, Elemental Status System |
| 27 | Session Rewards Screen (inferred) | UI | MVP | Not Started | — | Loot & Economy System, Dungeon Structure System, Save/Load & Persistence |
| 28 | Dungeon Hub (inferred) | UI | MVP (pre-dungeon loadout only; hub shop post-MVP) | Not Started | — | Save/Load & Persistence, Manuscript/Spell Unlock System, Loadout/Spell Slot System, Lobby/Session System |
| 29 | Dungeon Run Variation System (inferred) | Gameplay | Vertical Slice | Not Started | — | Dungeon Structure System, Enemy AI System, Loot & Economy System |
| 30 | Matchmaking System | Networking | Alpha | Not Started | — | Networking Foundation, Lobby/Session System |

---

## Categories

| Category | Description | Typical Systems |
|----------|-------------|-----------------|
| **Core** | Фундаментальные системы, от которых зависит всё | Input, Camera, Player Controller, Pixelation Rendering |
| **Gameplay** | Системы, которые делают игру интересной | Spell Casting, Elemental Status/Synergy, Enemy AI, Combat, Dungeon Structure |
| **Networking** | Мультиплеер и сессионное управление | Networking Foundation, Lobby/Session, Matchmaking |
| **Progression** | Рост игрока во времени | Manuscripts, Character Progression, Spell Unlocks |
| **Economy** | Создание и потребление ресурсов | Loot & Economy |
| **Persistence** | Состояние между сессиями | Save/Load |
| **UI** | Информация для игрока | Combat HUD, Target Feedback, Session Rewards, Dungeon Hub |
| **Presentation** | Визуальный фидбек игровых событий | VFX & Spell Effects |
| **Audio** | Звук и музыка | Audio System |

---

## Priority Tiers

| Tier | Definition | Target Milestone | Design Urgency |
|------|------------|------------------|----------------|
| **MVP** | Необходим для 25-минутного кооп-захода в данжн — без этого нельзя протестировать "весело ли?" | Первый играбельный прототип (4-6 месяцев) | Design FIRST |
| **Vertical Slice** | Нужен для полноценного опыта с реплейабилити — вариация между заходами | Demo/Vertical Slice (8-12 месяцев) | Design SECOND |
| **Alpha** | Все функции в черновой форме, полный механический скоуп | Alpha (14-18 месяцев) | Design THIRD |
| **Full Vision** | Полировка, контентное наполнение, edge cases | Beta / Release (18-24 месяца) | По мере необходимости |

---

## Dependency Map

### Foundation Layer (нет зависимостей)

1. **Input System** — всё взаимодействие игрока с миром начинается здесь; FPS-прицеливание + биндинги заклинаний
2. **Camera System** — FPS от первого лица; зафиксировано в концепте; без неё невозможно тестировать каст
3. **Health & Damage System** — формулы урона и HP; 7 систем читают или пишут в эти данные
4. **Networking Foundation** ⚠️ BOTTLENECK — NGO + Unity Transport + Relay; listen-server; блокирует 8 систем; ADR-0001 написан (статус Proposed → нужно Accepted)
5. **Pixelation Rendering Pipeline** — кастомный URP пост-процесс; визуальная идентичность; независим от геймплея
6. **Audio System** — SFX/ambient/feedback фреймворк; независим от геймплея
7. **Save/Load & Persistence** — кросс-сессионный прогресс; без него манускрипты и снаряжение теряются

### Core Layer (зависят от Foundation)

1. **Player Controller** — FPS-движение, CharacterController, коллизии; синхронизирован по сети — depends on: Input System, Camera System, Networking Foundation
2. **Elemental Status System** ⚠️ BOTTLENECK — флаги Wet/Burning/Frozen/Electrified на врагах; `NetworkVariable<ElementalStatusFlags>` (ADR-0001) — depends on: Health & Damage System, Networking Foundation
3. **Spell Casting System** ⚠️ BOTTLENECK — FPS-прицеливание → каст → хит-детекшн; `CastSpellServerRpc` (ADR-0001) — depends on: Input System, Camera System, Elemental Status System, Networking Foundation
4. **Enemy AI System** — телеграфированные атаки, движение, визуальная читаемость уязвимостей; спавн через `NetworkObject.Spawn` (ADR-0001) — depends on: Health & Damage System, Networking Foundation
5. **Lobby/Session System** — friends-join, слоты 2-4 игроков, NGO-сессия — depends on: Networking Foundation
6. **Character/School Selection** — выбор стихийной школы; определяет доступный пул заклинаний — depends on: Lobby/Session System

### Feature Layer (зависят от Core)

1. **Elemental Synergy System** ⭐ ЭТО И ЕСТЬ ИГРА — бонус-эффекты при комбо состояний (вода+молния=Chain Shock) — depends on: Elemental Status System, Spell Casting System, Health & Damage System
2. **Target Feedback System** ⚠️ PRODUCTION RISK — индикатор текущей цели; без него синергии ломаются незаметно (находка прототипа 2026-08-26) — depends on: Input System, Camera System, Spell Casting System
3. **Loadout/Spell Slot System** — 3 активных слота; переключение в бою; расширяется манускриптами — depends on: Spell Casting System, Character/School Selection, Save/Load & Persistence
4. **Spectator/Death System** — смерть → спектатор до конца боя; вайп → чекпойнт — depends on: Health & Damage System, Networking Foundation, Player Controller
5. **Checkpoint System** — чекпойнты между аренами; логика вайп-респавна — depends on: Save/Load & Persistence, Health & Damage System
6. **VFX & Spell Effects System** ⚠️ DESIGN RISK — состояния врагов читаемы без HUD через particle-цвета; это механика, не полировка — depends on: Spell Casting System, Elemental Status System, Pixelation Rendering Pipeline
7. **Dungeon Structure System** ⚠️ BOTTLENECK — комнаты (арены/коридоры/пазлы), порядок, босс-триггер; 5 систем зависят — depends on: Enemy AI System, Player Controller, Checkpoint System
8. **Boss System** — финальный босс данжна с уникальными фазами — depends on: Enemy AI System, Dungeon Structure System
9. **Puzzle Room System** — механика puzzle-секций (MVP: 1 секция); пауза в темпе — depends on: Player Controller, Dungeon Structure System
10. **Loot & Economy System** — таблицы дропа, золото, снаряжение, манускрипты из данжна — depends on: Dungeon Structure System, Save/Load & Persistence
11. **Manuscript/Spell Unlock System** — нахождение → разблокировка нового заклинания — depends on: Loot & Economy System, Loadout/Spell Slot System
12. **Character Progression/Level System** — уровень персонажа → пассивные бонусы (MVP: минимальная реализация) — depends on: Save/Load & Persistence, Health & Damage System

### Presentation Layer (зависят от Feature)

1. **Combat HUD** — кулдауны, HP, активные состояния врагов, слоты заклинаний — depends on: Spell Casting System, Health & Damage System, Elemental Status System
2. **Session Rewards Screen** — экран наград после данжна (лут + манускрипты) — depends on: Loot & Economy System, Dungeon Structure System, Save/Load & Persistence
3. **Dungeon Hub** — лобби/управление loadout перед заходом (MVP: только loadout; hub-магазин post-MVP) — depends on: Save/Load & Persistence, Manuscript/Spell Unlock System, Loadout/Spell Slot System, Lobby/Session System

### Polish Layer (post-MVP)

1. **Dungeon Run Variation System** — рандомизация врагов/лута между заходами для реплейабилити — depends on: Dungeon Structure System, Enemy AI System, Loot & Economy System
2. **Matchmaking System** — публичный matchmaking; MVP использует только friends-join — depends on: Networking Foundation, Lobby/Session System

---

## Recommended Design Order

| # | Система | Приоритет | Слой | Agent(s) | Оценка | Примечания |
|---|---------|-----------|------|----------|--------|------------|
| 1 | Input System | MVP | Foundation | gameplay-programmer | S | ✅ Retrofit завершён (2026-08-26): CastSpell/ScrollSpell вместо Attack, Target Feedback как provisional-зависимость, сетевая граница по ADR-0001 |
| 2 | Camera System | MVP | Foundation | gameplay-programmer | S | ✅ Спроектирован (2026-08-26): тело=yaw/камера=pitch, Cinemachine Impulse для shake/FOV-кика, spectator-хук вынесен в отдельную систему |
| 3 | Health & Damage System | MVP | Foundation | systems-designer | S | ✅ Спроектирован (2026-08-26): server-authoritative currentHP/maxHP по паттерну ADR-0001, maxHP — внешние данные, synergy_damage_multiplier + hp_clamp формулы, defense намеренно отложен |
| 4 | **Networking Foundation** | MVP | Foundation | network-programmer | L | ⚠️ КРИТИЧЕСКИЙ bottleneck; ADR-0001 написан и **Accepted** (2026-08-26) — начать GDD с ADR как основой, программирование разрешено |
| 5 | Pixelation Rendering Pipeline | MVP | Foundation | unity-shader-specialist | L | Требует технический спайк: render-target downscale vs shader-пикселизация vs vertex snapping |
| 6 | Audio System | MVP | Foundation | audio-director | S | SFX-фреймворк для каждой школы и комбо-фидбека |
| 7 | Save/Load & Persistence | MVP | Foundation | engine-programmer | M | Кросс-сессионный прогресс манускриптов и снаряжения |
| 8 | Player Controller | MVP | Core | gameplay-programmer | S | FPS CharacterController + сетевая синхронизация |
| 9 | **Elemental Status System** | MVP | Core | systems-designer | M | ⚠️ Bottleneck; `NetworkVariable<ElementalStatusFlags>` из ADR-0001; проектировать до Spell Casting |
| 10 | Lobby/Session System | MVP | Core | network-programmer | M | NGO-сессия, friends-join, слоты игроков |
| 11 | Character/School Selection | MVP | Core | game-designer | S | Простая, но нужна до Loadout |
| 12 | **Spell Casting System** | MVP | Core | game-designer, gameplay-programmer | M | ⚠️ Bottleneck; FPS-каст + хит-детекшн; `CastSpellServerRpc`/`TriggerSynergyClientRpc` из ADR-0001 |
| 13 | Enemy AI System | MVP | Core | ai-programmer | L | Враги должны телеграфировать уязвимости (Pillar 4); VFX-состояния читаемы без HUD |
| 14 | **Elemental Synergy System** | MVP | Feature | game-designer, systems-designer | M | ⭐ Главная механика игры; проектировать как первая Feature-система |
| 15 | Target Feedback System | MVP | Feature | ux-designer | S | ⚠️ Production risk из прототипа: без прицела комбо незаметно ломаются |
| 16 | Loadout/Spell Slot System | MVP | Feature | game-designer | S | 3 слота + переключение + расширение манускриптами |
| 17 | Spectator/Death System | MVP | Feature | game-designer | S | Смерть → спектатор; вайп → чекпойнт (Pillar 2) |
| 18 | Checkpoint System | MVP | Feature | systems-designer | S | Чекпойнты между аренами |
| 19 | **VFX & Spell Effects System** | MVP | Feature | technical-artist, unity-shader-specialist | L | ⚠️ Design risk: состояния врагов — механика, не декор; требует тесной работы с Enemy AI GDD |
| 20 | **Dungeon Structure System** | MVP | Feature | level-designer | L | ⚠️ Bottleneck; 1 данжн: 3-4 арены + 1 пазл + 1 босс; 5 систем зависят |
| 21 | Boss System | MVP | Feature | game-designer, ai-programmer | M | Финальный босс с уникальными фазами |
| 22 | Puzzle Room System | MVP | Feature | game-designer | S | 1 секция; пауза в темпе; без платформинга (post-MVP) |
| 23 | Loot & Economy System | MVP | Feature | economy-designer | S | Намеренно минимальная: дроп-таблицы + манускрипты из данжна |
| 24 | Manuscript/Spell Unlock System | MVP | Feature | game-designer | S | Находка → разблокировка нового заклинания |
| 25 | Character Progression/Level System | MVP (minimal) | Feature | systems-designer | S | Уровень → пассивные бонусы (урон, мана); не новые заклинания |
| 26 | Combat HUD | MVP | Presentation | ui-programmer, ux-designer | S | Кулдауны, HP, активные состояния врагов |
| 27 | Session Rewards Screen | MVP | Presentation | ui-programmer | S | Экран наград после завершения данжна |
| 28 | Dungeon Hub | MVP (loadout only) | Presentation | game-designer, ui-programmer | S | Pre-dungeon loadout; магазин post-MVP |
| 29 | Dungeon Run Variation System | Vertical Slice | Feature | game-designer | M | Рандомизация врагов/лута для реплейабилити |
| 30 | Matchmaking System | Alpha | Polish | network-programmer | M | Публичный matchmaking; MVP = friends-join only |

---

## Circular Dependencies

✅ Не обнаружены. Elemental Status System и Spell Casting System тесно связаны (Casting
читает и записывает статусы), но это не цикл: Elemental Status предоставляет API и
networked state, Spell Casting его использует. Архитектура определена в ADR-0001.

---

## High-Risk Systems

| Система | Тип риска | Описание | Митигация |
|---------|-----------|----------|-----------|
| **Networking Foundation** | Технический | Первый 3D-проект разработчика + сетевой мультиплеер 4-5 игроков; синхронизация стихийных статусов в реальном времени — главный риск всего проекта | ADR-0001 написан (NGO + Transport + Relay); перевести в Accepted до программирования; запустить networking spike как первый технический тест |
| **Elemental Synergy System** | Дизайн | Гипотеза о "спонтанном открытии комбо двумя независимыми игроками" технически подтверждена в прототипе, но не проверена реальными двумя игроками | Провести реальный 2-player тест прототипа `co-op-spellcasting-concept` до финализации этого GDD |
| **Target Feedback System** | Дизайн | Без видимого прицела игрок не знает в кого попадёт заклинание — комбо ломаются без ошибки и без фидбека (подтверждено прототипом) | Спроектировать в числе первых Feature-систем; не откладывать как UI-деталь |
| **VFX & Spell Effects System** | Дизайн/Арт | Состояния врагов (мокрый/горящий/замороженный) должны быть читаемы без HUD; pixel-art стиль может скрывать мелкие состояния | Проектировать совместно с Enemy AI GDD; явно прописать требования читаемости в Acceptance Criteria; тест на читаемость с playtester-ами без объяснений |
| **Pixelation Rendering Pipeline** | Технический | Точная техника (render-target downscale vs shader-пикселизация vs vertex snapping) не определена и не протестирована | Провести технический спайк до написания GDD; зафиксировать в отдельном ADR |
| **Enemy AI System** | Дизайн | Враги — единственный защитный механизм игрока (нет dodge/block); пропущенный телеграф = неизбежный урон без ответа | Проектировать с явными требованиями к читаемости телеграфа (тайминг, анимация, аудио); тест на читаемость с новыми игроками |
| **Dungeon Structure System** | Скоуп | Первый 3D-проект + ручной дизайн комнат (не процедурный по Anti-Pillar) = серьёзная недооценка трудозатрат на 1 данжн | Явно подсчитать стоимость первого данжна по количеству комнат и энкаунтеров в этом GDD |

---

## Progress Tracker

| Метрика | Количество |
|---------|-----------|
| Всего систем идентифицировано | 30 |
| Design docs начато | 3 (input-system.md — retrofit; camera-system.md, health-damage-system.md — новые) |
| Design docs проверено | 0 (запустите `/design-review` в свежей сессии для каждого) |
| Design docs утверждено | 0 |
| MVP-систем спроектировано | 3/28 |
| Vertical Slice систем спроектировано | 0/1 |
| Alpha систем спроектировано | 0/1 |

---

## Next Steps

- [x] Согласовать перечисление систем
- [x] Согласовать карту зависимостей
- [x] Согласовать порядок проектирования
- [x] ADR-0001 переведён Proposed → Accepted (2026-08-26)
- [x] Input System retrofit завершён (2026-08-26) — CastSpell/ScrollSpell, Target Feedback dependency, сетевая граница
- [x] Camera System спроектирован (2026-08-26) — `design/gdd/camera-system.md`
- [x] Health & Damage System спроектирован (2026-08-26) — `design/gdd/health-damage-system.md`
- [ ] Провести реальный 2-player тест прототипа `co-op-spellcasting-concept` до финализации Elemental Synergy GDD
- [ ] Bottleneck-first порядок (по запросу пользователя): следующая — Networking Foundation (`/design-system networking-foundation`), затем Elemental Status System, Spell Casting System, Target Feedback System, Elemental Synergy System
- [ ] Запускать `/design-review` на каждом завершённом GDD
- [ ] Запустить `/gate-check pre-production` после завершения MVP-систем
