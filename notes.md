# Constraint Programming in pursuit of the holy grail - barak

- CSP Problem definition
-

# NAPADY

## Zavedu metriku _Moves_.

### Definice

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

## Prozkoumat vice ten prostor

### S jakou pravdepodobnosti nahodne reseni splnuje?

- v zavislotni na poctu incidentu, poctu sanitek

### Jak dobri jsou sousede nejakeho splnujiciho reseni?

1. Jak moc se meni Fitness po provedeni vsechno moves?
1. Jak rychle klesa fitness v zavislosti na poctu provedenych canonical moves?

# POSTREHY

## Nelze v polynomialnim case rict, zda je reseni optimalni, jen ze se jedna o reseni.

- Nebudeme mit tedy nikdy garanci, ze reseni je nejlepsi - prostor je prilis velky a vzdy muze existovat v neprohledane casti lepsi reseni.
- muzeme se ale aspon ptat s jakou pravdepodobnosti je vygenerovane reseni to nejlepsi?
  - jedine odvijet od velikosti prohledaneho podprostoru, ten bude vzdy polynomialni, takze hrozne maly.
    - cili je dulezite jakym zpusobem prohledavas
    - najit vztah mezi shiftPlans
