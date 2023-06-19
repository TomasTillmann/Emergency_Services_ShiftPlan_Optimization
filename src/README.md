# Poznamky
## Casova slozitost Exhaustive Optimizeru
* $a$ je pocet startingTimes, $b$ je pocet allowedDurations, $S$ je mnozina shiftu a $I$ je mnozina uplne vsech incidentu pres vsechny incidentsSets.
* Pocet prohledanych ShiftPlanu je $(a \times b + 1)^{|S|}$
	* $a$ a $b$ udavaji vetvici faktor, protoze uvnitr jedne hladiny zkousim vsechny moznosti starting times a durations
	* +1, protoze shift jeste muze byt nepouzity (0s-0s)
	* $|S|$ udava hloubku stavoveho prostoru

Celkem odvedene prace je:
$$
(a \times b + 1)^{|S|} * |I|
$$

* Pro kazdy jeden shiftPlan musim spusti simulaci, ta je linearni vuci $|I|$

# V reci COP
Minimalizujeme cost function $cost(ShiftPlan) -> double$.

Constrainty jsou:
1. simulace musi na shiftPlane projit: $simulation(ShiftPlan) -> bool = true$

ShiftPlan neni nic jineho nez $n$ intervalu - $S$, ktere musi zacinat a trvat podle $t_a$ a $t_d$.

Preformulovane:
$min(cost(S))$


Constraints:
1. $simulation(S) = true$

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

## Tipy
Prvni napsat ten kod a pohrat si s temi metodami atd ... az pak spise zacit psat. Pomuze mi to i pri tom co dat do te faze teorie a jak to cele uvest.
Nemuzu delat uvod do neceho co jeste nevim jak bude probihat.

# TODO
* Benchmarkuj simulaci
* nejaky chytrejsi optimizer
* zrefaktorovat starting time a starting location do jedne metody - maji vicemene stejny kod a nedodrzuji open closed princip

## Optimalizace
* Spocitej si nejakou mapu, kde budes mit ke kazdemu bodu / ctverecku v  prostoru, kde je nejblizsi nemocnice
	* tedka v kazdem kroku simulace prochazim vsechny nemocnice abych nasel tu nejblizsi
* Zrefaktoruj step v simulaci at se shifts prochazi pouze jednou ne dvakrat
	* ted to delam dvakrat, muzu to delat jen jednou
* Cachuj si nejak ty PlannableIncidenty, vytvarim je jak v GetHandling, tak v GetBetter
	* a prave v nich se furt prochazi i ty nemocnice

