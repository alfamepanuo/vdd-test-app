# VDD Test App

A .NET 9 starter application combining an ASP.NET Core minimal JSON API with a Blazor interactive-server admin placeholder. The current API includes `GET /api/health`, which returns `{"status":"ok"}`.

## Nykyisen järjestelmän toiminnallisuuskartoitus

Tämä kartoitus kuvaa nykyisestä lähdekoodista varmennettavan toiminnan uuden järjestelmän suunnittelun ja migraatiokartoituksen lähtötiedoksi. Järjestelmä on aloituspohja: siinä ei ole toteutettu liiketoiminnan tietoja, työnkulkuja, käyttäjätunnistusta eikä käyttöoikeuksia. Alla oleva siirtopäätös on alustava eikä korvaa liiketoiminnan vahvistusta.

### Toiminnallisuudet

| Toiminnallisuus | Kuvaus nykyjärjestelmässä | Käyttäjärooli | Siirtopäätös |
| --- | --- | --- | --- |
| Terveystarkistus | `GET /api/health` palauttaa HTTP 200 -vastauksen ja JSON-rungon `{"status":"ok"}`. Päätepiste ei edellytä tunnistautumista. | API-kutsuja; roolia ei tunnisteta eikä tarkisteta. | Ei siirretä liiketoimintatoimintona. Uuden järjestelmän tekninen terveystarkistus ratkaistaan erikseen. |
| Käännösten ylläpidon käyttöliittymän paikkamerkki | Blazor-sivulla `/` näytetään otsikko "Translations admin". Käännösten listaus- tai muokkaustoimintoja ei ole toteutettu. | Ylläpitäjä on vain sivun otsikon implikoima; roolia tai käyttöoikeutta ei ole toteutettu. | Paikkamerkkiä ei siirretä. Käännösten ylläpitotarve ja roolit on vahvistettava ennen uuden järjestelmän määrittelyä. |
| Sovelluksen sisäinen tapahtumankäsittely | Tallennuksen jälkeen aggregaattien domain eventit välitetään rekisteröidyille käsittelijöille. Integration event -abstraktion julkaisu kutsuu rekisteröityjä käsittelijöitä prosessin sisällä. Konkreettisia tapahtumia, käsittelijöitä tai liiketoimintakäyttöä ei ole toteutettu. | Ei suoraa käyttäjäroolia; tekninen sovelluskomponentti. | Ei siirretä sellaisenaan. Tapahtumapohjaisen käsittelyn tarve ja tapahtumat määritellään uuden järjestelmän käyttötapausten perusteella. |
| Tietojen tallennuskehys | EF Core -yhteys käyttää SQLiteä (`app.db` oletuksena). Tuotantokoodissa ei ole määritelty tallennettavia toimialan entiteettejä tai `DbSet`-joukkoja. | Ei suoraa käyttäjäroolia; sovellus tallentaa tekniset tiedot. | Ei nykyistä toimialan dataa siirrettäväksi. Uuden järjestelmän tallennettavat tiedot on määriteltävä erikseen. |

### Ulkoiset rajapinnat ja integraatiot

| Rajapinta tai integraatio | Suunta | Tarkoitus ja rajaus |
| --- | --- | --- |
| JSON-terveystarkistus (`GET /api/health`) | Sisäänpäin: API-kutsuja -> sovellus | Vastaanottaa terveystarkistuspyynnön ja palauttaa tilan. Tämä on ainoa toteutettu JSON API -reitti; liiketoimintarajapintoja ei ole. |
| Blazor-käyttöliittymä (`/`) | Sisäänpäin: selain -> sovellus | Tarjoaa ylläpidon paikkamerkkisivun. Ulkoisia palvelukutsuja tai toiminnallista ylläpitoa ei ole toteutettu. |
| Application Insights | Ulospäin sovelluksesta, vain jos `ApplicationInsights:ConnectionString` on asetettu | Sovelluksen trace-telemetria lähetetään määritettyyn Application Insights -kohteeseen. Oletusarvoisesti yhteysmerkkijono on tyhjä, eikä integraatio ole silloin käytössä. |

SQLite ja lokitus konsoliin sekä paikallisiin kiertäviin tiedostoihin ovat sovelluksen sisäisiä/toteutusympäristön palveluita, eivät ulkoisia järjestelmäintegraatioita. Integration event -julkaisu tapahtuu prosessin sisällä eikä muodosta ulkoista yhteyttä. Muita ulkoisia integraatioita ei lähdekoodista löytynyt.

### Keskeinen datamalli

| Entiteetti tai tyyppi | Kentät ja tarkoitus | Viittaukset |
| --- | --- | --- |
| `Entity` (abstrakti) | `Id`: kokonaislukutunniste, jonka oletusarvoinen EF Core -käyttö tuottaa tietokantarivin avaimena. | Ei konkreettisia tuotantoentiteettejä eikä viittauksia toisiin entiteetteihin. |
| `AggregateRoot` (abstrakti, perii `Entity`-tyypin) | Peritty `Id`; lisäksi prosessin aikainen `DomainEvents`-kokoelma ja sen tyhjennys. Tapahtumakokoelma ei ole määritelty pysyväksi dataksi. | Perii `Entity`-tyypin; ei muita entiteettiviittauksia. |
| `HealthResponse` (API-vastaus, ei entiteetti) | `Status`-merkkijono, jonka arvo terveystarkistuksessa on `ok`. | Ei tietokantaviittauksia. |

Tuotannon `AppDbContext` ei määritä yhtään `DbSet`-entiteettiä eikä konfiguroi tauluja tai vierasavaimia. Siten nykyisestä tuotantodatamallista ei voida johtaa migroitavia tietueita tai entiteettien välisiä viittauksia. Testien esimerkkientiteetit ovat testikohtaisia eivätkä kuulu sovelluksen datamalliin.

### Avoimet kysymykset

- Mitä liiketoimintaprosesseja ja toimintoja nykyisen aloituspohjan ulkopuolella on tarkoitus korvata? Niitä ei voi päätellä tästä koodista.
- Onko käännösten ylläpito todellinen vaatimus? Jos on, mitä käännöksiä, kieliä ja toimintoja tarvitaan, ja mikä käyttäjärooli saa niitä hallita?
- Mitkä käyttäjäryhmät ja tunnistautumis-/käyttöoikeussäännöt uuden järjestelmän tulee toteuttaa? Nykyjärjestelmässä ei ole tunnistautumista.
- Onko vanhasta järjestelmästä siirrettävää dataa jossakin tämän sovelluksen ulkopuolisessa tietolähteessä? Tässä toteutuksessa ei ole toimialatauluja.
- Onko Application Insights käytössä nykyisessä ympäristössä, mikä kohde on käytössä ja pitääkö vastaava telemetria säilyttää?
- Tarvitaanko uuden järjestelmän terveystarkistus sekä domain- tai integraatiotapahtumien käsittely? Nykyinen tapahtumajulkaisu on vain prosessin sisäinen abstraktio.

## Structure

The solution follows inward-facing project dependencies: `Domain` contains core entities and event contracts; `Application` defines use-case and event-handler abstractions; `Infrastructure` provides SQLite/EF Core and in-process event services; and `Web` is the composition root. Web features are organized as vertical slices, with each feature owning its endpoints and contracts. The Blazor home page is a placeholder for the admin UI.

Domain events raised by aggregates are dispatched to their handlers after a successful unit-of-work save. Integration events are published through an abstraction with an in-process handler implementation as the default.

## Logging and configuration

The web host uses Serilog behind `ILogger`, writing to the console and rolling files under `logs/`. An Application Insights sink is enabled only when `ApplicationInsights:ConnectionString` is configured. Settings in `appsettings.json` can be overridden with environment variables; no credentials are included in the repository.

## Build and tests

Run these commands from the repository root:

```sh
dotnet restore
dotnet build --no-restore
dotnet format --verify-no-changes
dotnet test --no-restore --logger "junit;LogFilePath=$PWD/reports/unit.xml"
```

The xUnit suite covers the health endpoint, the prerendered Blazor page, in-memory SQLite, event dispatch and publishing, and project dependency boundaries. The test command writes its JUnit XML report to `reports/unit.xml`.
