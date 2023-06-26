# Constraint Programming in pursuit of the holy grail - barak

- CSP Problem definition
-

# NAPADY

## Construction search

- zatim pouzivam jen local search s metaheuristikou
- Zacnu s prazdnym shift planem a budu postupne prohledavat sousedy uplne stejne
  - mohlo by se chovat lepe, protoze se budu priblizovat zespoda, cili jedny z prvnich validnich reseni ktere najdu by mohli mit nejlevnejsi cenu

## Zavedu metriku _Moves_.

Mejme $s1 \in S$ a $s2 \in S$. Metrika _Moves_ $m: m(s1, s2)$ je pocet _moves_, ktery je treba provest, abychom se z
$s1$ dostali do $s2$ (nebo naopak), aniz bychom provadeli _moves_, ktere se navzajem neguji (napr. (Shorter, 2), ..., (Longer, 2)).

1. $m(s1, s1) = 0$

   - Nemusim delat zadne tahy, cili 0.

1. if $s1 != s2 \implies m(s1, s2) > 0$

   - Pokud jsou ruzne, tak musim vzdy udelat alespon jeden move.

1. $m(s1, s2) = m(s2, s1)$

   - Mejme cestu _moves_: $m_1, m_2, ..., m_n$ velikosti $n$ z $s1$ do $s2$.
   - Zkonstruujeme cestu $neg(m_1), neg(m_2), ... $.
   - To je cesta z $s2$ do $s1$ stejne delky.

1. presne ted nevim tbh

Jeste je treba dokazat ze aspon jedna takova cesta vzdy existuje.
Pak ze vsechny takove cesty maji stejnou velikost. Neni totiz prave jedna.

Ale podle me to je metrika proste.

### K cemu se mi hodi metrika?

Hodne se mi hodi. Muzu podle toho volit pocet iterations v local search algoritmech.

Napriklad: Mam 3 mozne intervaly pro kazdou sanitku, tech je treba 5. Muze byt libovolne $k$ a $n$.
Jsem schopny najit nejvetsi moznou vzdalenost v tomto prostoru.

Aby byli od sebe shift plans co nejvzdalenejsi pod touto metrikou, musim udelat co nejvice kanonickych tahu abych se z jednoho dostal na druhy.

Jelikoz kazdy tah ovnlivni prave jeden interval, tak abych nasel nejvzdalenejsi shift plan, tak mi staci najit dva od sebe nejvzdalenejsi intervaly.
Pak uz jenom jednomu shift priradim vsude ten jeden interval a druhemu ten druhy.

Najit dva nejvzdalenejsi intervaly je ez, proste vezmu jeden interval co nejvice na "kraji", cili nemuzu jit napr. uz nijak doleva ani zkratit (0s-0s).
A pak najdu ktery je co nejvice vpravo a nejde zvetsit (napr. 12h-12h). Operace posun doleva - doprava a zkratit-zdlouzit jsou navzajem negace, cili proto zvollim takhle.

Pak tyhle intervaly nasazim kazde sanitce. Vzdalenost pak je suma pres pocet tahu pro kazdou sanitku (lepe to nejde).

Oznacme tento maximalni pocet tahu $m$. Pokud zvolime iterations = $m$, tak vime, ze mame sanci na globalni minimum narazit. Je dosazitelne.
Cim vyssi iterations, tim mene perfektni muzu volit cestu, cili tim vice cest existuje, cili tim vetsi sance je ze globalni minimum najdu.

Muzeme tak dat spodni mez na pocet iterations, ktera nam garantuje pro kazdy zacinajici shift plan, ze globalni minimum je v dosahu.

## Prozkoumat vice ten prostor

### S jakou pravdepodobnosti nahodne reseni splnuje?

- v zavislotni na poctu incidentu, poctu sanitek
- S hodne malou

* napr. pustil jsem pro 30 sanitek a 3 incidenty s 16 moznymi intervaly bez typu sanitek. Udelalt jsem 10_000 random samplu. Ani jeden nesplnoval ...

### Jak dobri jsou sousede nejakeho splnujiciho reseni?

1. Jak moc se meni Fitness po provedeni jednoho moves?

   - zalezi jak moc je incidentu, pokud sanitky tak tak obslouzi vsechny incidenty, sousedi jsou stejne nebo hure ohodnoceni. Stejne ohodnocenych je pomalo.
   - Vetsinou ve stylu Earlier / Later, a to jeste na prazdem intervalu. Samozrejme pokud vytizene, zadny prazdny neni.

1. Jak rychle klesa fitness v zavislosti na poctu provedenych canonical moves?

### Jak rychle jsem od nahodneho reseni schopen najit validni shiftPlan?

- prohledavani sousedu a vybiranim nejlepsiho? Kolik to trva tahu?

### Jaka je prumerna velikost lokalniho minima?

- v zavislosti na incidentech / sanitkach atd ...

## Lepsi framework pro input

- file kde bude json a tim nastavim vstupni parametry, ten si naparsuju do nejakeho Input objektu

```cs
class Input {
    World World;
    Domain Domain;
}

```

## Nebehat simulaci vzdy na vsech incidents sets

Muzu zkusit vybrat jen par reprezentantu nahodne pro kazdy simulation run. Udelat z toho tak trochu dynamic constraint optimization problem.

Mohlo by vest vice k obecnejsim resenim a ne hard tailored pro danou incidents mnozinu.
Taky by to mohlo byt rycheljsi, zvlaste pokud incidents set je pocetnejsi.

## Mit nejakou heuristiku fitness

Cili nepobezim celou simulaci az do konce napriklad.

Jakmile napriklad success rate prilis nizky, tak prerusim a vratim.

Nebo pokud duration shiftPlan je mensi nez pozadavana minimalni hranice, viz nize, tak vratim nejake vysoke $n$.

# Annealing

- ruzne varianty, napr. vyberu uplne nahodne, ne souseda.
- dam pryc tu konstantu, viz str. 63 v mravencich. - jenom metropolis distribution.

# TS

- pry je jedna z nejlepsich metaheuristik
- da se jeste vylepsit, drzet si nejake elitni reseni a pak se k nim vracet, neco jako seznam vsech reseni co byli global best. Kdy se k nim ale vratit? Treba co $k$ prohledat jedno z elitnich reseni?
- misto toho, aby sis primo drzel to resni, muzes si pamatovat tah ktery k nemu vedl nebo tak.
- doporucuje Glover & Laguna 1997

# POSTREHY

## Nelze v polynomialnim case rict, zda je reseni optimalni, jen ze se jedna o reseni.

- Nebudeme mit tedy nikdy garanci, ze reseni je nejlepsi - prostor je prilis velky a vzdy muze existovat v neprohledane casti lepsi reseni.
- muzeme se ale aspon ptat s jakou pravdepodobnosti je vygenerovane reseni to nejlepsi?
  - jedine odvijet od velikosti prohledaneho podprostoru, ten bude vzdy polynomialni, takze hrozne maly.
    - cili je dulezite jakym zpusobem prohledavas
    - najit vztah mezi shiftPlans

## Popis problem

- Static Combinatorial optimization problem, with one constraint - musi projit simulaci, (asi neni constraint actually vubec).
- Intractable
  - dokazu tak, ze corresponding indecisive problem je NP tezky, viz kniha o mravencich
- Nejsme schopni nijak overit zda se jedna o optimalni reseni.

# A comparative study of Meta-heuristic Algorithms for solving QAP

## Plot

- Osa x bude konkretni vstup - (#sanitek, #incdentu), pripadne #depotu atd ...
- Osa y bude nejvic optimalni reseni
- Osa y bude execution time pro nalezeni toho reseni
- na jednom grafu se budou prekryvat, at to jde hezky videt

## Omezeni cost shiftPlan zespod

- Pro kazdy incident pres vsechny incidenty sectu:
  - OnSceneDuration
  - InHospitalDelivery
  - Cesta do nejblizsi nemocnice
    - vsechny sanitky maji stejnou rychlost, cili delku trvani jsem schopen odvodit ciste od polohy incidentu

* Tato suma mi udava minimalni dobu trvani, kterou musi mit kazdy validni shiftPlan

  - muzu dokazat kontradikci

* seshora

# ACO

- je constructive, takze muze byt zajimave porovnat s local search based
- vrcholy budou vsechny mozne intervaly
- $n$-partitni graf
- $n$ je pocet sanitek
- hrany vzdy z $n$ partity do $n+1$ z kazdeho do kazdeho
- stav mravence - cesta - pak reprezentuje prirazeni intervalu v $k$ vrstve do $k$ shifty
- constraints nejsou
  - neni nic jako maximalne jedna 12 hodinva smena, nebo ze maximalne musi mit shift plan v souctu nejake $n$ ...
  - dalo by se udelat lehce tak, ze mravenec si totiz pamatuju cestu, tak by v $N$ nemel jako dostupne sousedy vsechny z dalsi vrstvy ale jen ty co splnuji constraints
    - pokud by neslo, tak treba vyberes ten infeasible, ktery je nejmensi zlo, pak ale taky musis ohodnotit spravne feronomama, jakoze slepa ulicka vicemene

## Co bude heruistika na hrane?

- pridam ten shift, ktery za co nejlevnejsi cenu co nejvice fitness

  - muze byt pomale

- budu preferovat pridani vzdy tech levnejsich intervalu

* budu se snazit pridavat intervaly ktere zaplnuji casove diry doposud vytvoreneho shift planu

* budu mit minimalni hranici jak dlouho musi shiftPlan trvat a maximalni hranici a budu se snazit nacpat do tohohle pasma

  - minimalni hranice bude spodni mez delky shiftPlan
  - maximalni by mohl byt treba lehce nadsazena expected cost? Takze nejaky parametr nebo -
    - pustim local search nejaky, jen na par iteraci, podivam se kolik stoji nalezeny shift plan, mozna udelam par samplu - branch & bound? - a tohle zvolim jako horni mez
    - stejne local search potrebujes spustit na init feronomu

* kombinace - levnejsi, vyplnit diru, idealni pasmo

## Jak updatovat feronomy?

- mel bych hodnotit hrany
  - hodnocenim hrany rikam, jak dobre je po shiftu x vybrat shift y
  - to by mohlo resit zavislosti shiftu navzajem mezi sebou
  - tim vice ma hrana feronomomu, tim vicekrat dobre reseni obsahuje tuto dvojici shiftu
    - jenze me nezajimaji konkretni dvojice, ale celkovy vztah, ktery nevim jetli se prenasi nejak

* dava ale i smysl hodnotit vrcholy, mozna dokonce vetsi

  - tim rikam, jak dulezity je dany shift
  - cim vice ma feronomu, tim ve vice dobrych reseni je

* update udelam pak naraz, na kvalite daneho reseni

* nebudu updatovat jak tam poleze

  - to se jmenuje tzvn. Ant cycle

* str. 87 ACO - vzorecek
  - evaporuju
  - pak nastavim podle reseni
  - 1 / successRate

## Jak na zacatku incializiovat feronomy a proc?

- hodi se na zacatku inicializovat feronomy na neco jako expected hodnotu, kterou prvni mravenci ohodnoti svoje reseni
- Proc?
  - kdyz moc mala, treba = 0, tak mravenci jsou hodne rychle biased na prvni cesty, ktere ale byli vybrany random
  - pokud moc vyskoa, nekolik iteraci se ztrati, protoze nez prolezani mravencu prebije init ohodnoceni, ktere pomalu vyprchava, tak to muze trvat par iteraci
  * v TSP: #mravencu / NNH result

## Jak si mravenec vybere kam pujde?

    - str. 85, ACO, vzorecek
        * beres v potaz feronomy a heuristiku
    - musis vhonde zvolit alfa a beta
    - stejne tak musis vhodne zvolit init feronoms

## Co reprezentuji feronomy?

- cim vyssi, tim vic je zadouci po zvolenem shift planu x pouzit shift plan y, pro hranu (x,y)

## Co pak udelat kdyz dojdu do posledni partity $n$?

- jednoduse ukoncim tuto iteraci solution construction, cili zastavim toho mravence.

## Kdy simulaci zastavit? Kdy zastavit mravence?

- simulaci zastavim kdy chci - pocet interations, po dlouhem poctu iterations jsme na jednom miste atd ...
- mravence zastavim jakmile dojde do posledni vrsty, nasel totiz jedno reseni
  - muze byt ale mega spatne, napr. plan je nevalidni
  - ale jelikoz nemam zadne constraints, tak vsechna reseni jsou feasible
  - splnit tu simulaci neni constraint!!! jenom bude mit hodne spatne hodnoceni!!!

## Jak se vyhnout stagnaci?

- MMAS - udava spodni hranici na feronomy, tim je garantova vzdy nejaka minimalne explorace - nenastane 0 0 0 0 0 1 0 0 0 0 0 distribuce
  - taky reinicializuje feronome trails, cimz zase zvysuje exploration

## Co je blbe doposud

- feromony jsou nejake nizke furt, mnohem vetsi vahu ma stale heuristika, coz je blbe, protoze se nijak nevyuziva past experience
- duvod vypada ze je to, ze narazit nahodou na alespon validni je hodne mala, cili se mravenci neustale jen pohybuji v prostoru nevalidnich reseni
- mozna zkusit neco jak v prvnich k iteracich dat local search na nejlepsiho mravence na hodne iteraci a pak uz jen zlehka nebo vubec
  - tim bych mel pro kazdeho mravence alespon jedno nejake rozumnejsi reseni, nez jenom ta co ziskavam naslepo pres heuristiku
  - pak by i feronomy byli silnejsi (stejn bych mel zmenit tu funkci at je lepsi)
  - nevyhoda muze byt ze se stucknu na nejakych lokalnich optimech

## Ostatni

- zminit ze se jedna i o model based, ne jako samlple based
- cili ze se vytvari nejaky pravdepodobnostni model, ktery se snazi popsat kde se v prostoru nachazi dobra reseni

# Graf jak rychle local search konverguje k dobrym resenim

- na ose x iterations
- na ose y globalBestFitness

# Generovani dat

- nechci to asi udelat uplne uniforme, napriklad v noci, by se melo dit mene incidentu
- a pak ani uniforme prostorove, napriklad dopoledne / rano by se melo dit vice incidentu na cesto do skoly / prace

  - stejne pak odpoledne
  - a behem dne jsou lidi v prace tak uz normalne uniforme

- distance funkce by mela byt predpocitana na nejake zdiskretizovane ctverecky - distance matrix pak
  - tu si pak muzes kdo chce vyplnit jak chce, treba realnymi daty
  - a menit jeji velikost
  - zmenenim velikost zmenis prostor, kde se muzou dit incidenty
