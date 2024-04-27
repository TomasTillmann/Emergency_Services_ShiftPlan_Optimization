# Osnova
Osnova
Introduction
* uvede cloveka do problematky - jakou mame motivaci a jake mame cile - jednoduse

Popis problemu
* vice do hloubky

Popis Optimization problemu
* teorie obecne
* vsechny terminy
* budu se pak z tohohle odrazet
* muzu tam dat i neco co treba nebudu primo pouzivat, zalezi jak je to zajimave / provazane s tim co realne delam

Popis simulace
* nejaky introduce do problemu urcite

Jen prospesne pokud pridam i nejakou kapitolu na popis moji architektury toho kodu, designu, paralelizace nebo nejaka optimalizace, popis i mapovani te domeny do mych struktur, casova a pametova slozitost
* jak to je nejak naprogramovane

Vysledky
* porovnani tech pristupu, nejake grafy, jak co rychle konverguje
* zkouset ruzne situace, i extremni, napr. strasne moc nemocnic, depotu, blizko u sebe, hrozne moc incidentu / shiftu atd ...
	* sledovat jak se jednotlive metody chovaji v techle situacich

Conclusion
* finalni porovnani, asi i nejaky ?vlastni nazor?, co je nejlepsi podle me

40-60 stranek, 30 je malo.
Kod neni tak dulezity. Dulezity je hlavne ten text.

# Napady co udelat
## Zoptimalizovat simulaci, musi byt fakt mega rychla
- netvorit zbytecne veci na halde, recyklovat pamet

## Ordinal Optimization
- vypada ze se spis zabyva u simulaci ktere jsou stochasticke, a nejak meri samply
- muzu vygenerovat nahodne nejaky plan
  - nebo nejaky prumerny treba
  - nebo skoro prazdny
  - nebo skoro plny
- vylepsit ho na max a najit lokalni minimum hill climbem
- udelat tohle hodnekrat, tak ziskam ten ordinal set
- reknu ze optimum je nejlepsi z ordinal setu
- nebo muzu pak v budoucnu pouzit na krizeni v gen. algoritmech


