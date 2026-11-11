# Příprava prvního zveřejnění

Místní plán, nikoli potvrzení zveřejnění nebo schválení podepisování.

1. Zkontrolovat `BasicEdu-1.0-source-public.zip`, licenci MIT a označení autora.
   „BASIC EDU contributors“ neuvádí celé osobní jméno.
2. Potvrdit účet GitHub a adresu nového veřejného repozitáře. Správce zapne MFA.
3. Nahrát jen připravené zdroje; nezveřejňovat SDK, bin/obj, .vs, klíče nebo osobní programy.
4. Spustit GitHub workflow a zkontrolovat testy, ZIP a metadata vlastních EXE/DLL.
   Workflow nahraje unsigned artifact pro kontrolu; nevytváří veřejné vydání.
5. Po kontrole a souhlasu správce vytvořit předběžné vydání s popisem funkcí,
   upozorněním na nepodepsaný stav a odkazem na Code signing policy.
6. Požádat přes https://signpath.org/ o posouzení projektu. Nadace požaduje vydaný,
   dokumentovaný, udržovaný projekt a ověřitelnou reputaci. Přijetí není nárokové.
7. Po schválení doplnit veřejné role, požadovanou atribuci, konfiguraci artefaktů
   a ruční schvalování podpisů. Podepisování propojit podle přidělené konfigurace.
8. Ověřit podpisy vlastních souborů a spuštění celého balíčku se zapnutou ochranou Windows.
   Závislosti se nepodepisují jménem tohoto projektu.

## Podklady do žádosti (neodesláno)

- Název: BASIC EDU. Bezplatné české výukové prostředí pro číslovaný BASIC na Windows.
- C# / .NET 10 / WPF; editor AvalonEdit (MIT). Licence projektu MIT.
- Editor, interpret, UTF-8 soubory, 31 ukázek, krokování, proměnné a virtuální LED.
- Bez vlastní telemetrie a síťové komunikace; podrobnosti v PRIVACY.md.
- Vlastní soubory k podpisu: BasicEdu.exe, BasicEdu.dll a BasicEdu.Core.dll.
- Repozitář, první vydání a veřejná identita správce: doplnit po schválení zveřejnění.
- Navržený správce/recenzent/schvalovatel: Petr.

Před odesláním znovu ověřit podmínky: https://signpath.org/terms.html.
