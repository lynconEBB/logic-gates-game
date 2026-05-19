# LogicGatesGame

## 1. Telemetria

A telemetria de uma sessão é registrada apenas quando o jogador resolve o
circuito. Sessões abandonadas não geram registro.

### 1.1 Dados coletados por sessão

| Campo | Significado |
|---|---|
| `sessionId` | Identificador único da sessão de jogo. |
| `createdAtUtc` | Data/hora (UTC) de início da sessão. |
| `time` | Tempo de conclusão, em segundos inteiros. |
| `circuitExpression` | Expressão booleana do circuito jogado. |
| `appVersion` | Versão do aplicativo. |
| `platform` | Plataforma (ex.: Android, Windows). |
| `deviceModel` | Modelo do dispositivo. |
| `gates` | Quantidade de portas (gates) encaixadas. |
| `disconnections` | Número de desconexões entre nós. |
| `connectionCanceled` | Fios soltos fora de qualquer conector. |
| `connectionFailed` | Fios soltos em um conector que não podia aceitá-los. |
| `connectionSuccessful` | Conexões válidas entre nós. |
| `connections` | Total de conexões (canceladas + falhas + bem-sucedidas). |
| `score` | Pontuação final (ver seção 2). |

### 1.2 Captura de movimento

Durante a partida, a posição e a rotação são amostradas a cada **0,1 s** para:

- **Cabeça** (câmera principal)
- **Controle esquerdo**
- **Controle direito**

Cada amostra guarda posição `(x, y, z)` e rotação em quaternion `(x, y, z, w)`,
junto com o índice da amostra e o tempo decorrido.

---

## 2. Pontuação

A pontuação parte de **1000** e diminui conforme os erros e o excesso em
relação aos valores ideais do circuito.

### 2.1 Pesos

| Penalidade | Peso |
|---|---|
| Tempo (por segundo acima do ideal) | `0,02` |
| Portas em excesso (acima do ideal) | `1,5` |
| Conexões em excesso (acima do ideal) | `0,75` |
| Conexão com falha | `2,0` |
| Conexão cancelada | `0,75` |
| Desconexão | `1,0` |

### 2.2 Fórmula

```
penalidadePortas    = max(0, portasUsadas    - portasIdeais)    * 1,5
penalidadeConexoes  = max(0, conexoesFeitas   - conexoesIdeais)  * 0,75
penalidadeTempo     = max(0, tempoConclusao   - tempoIdeal)      * 0,02
penalidadeFalhas    = conexoesComFalha   * 2,0
penalidadeCanceladas= conexoesCanceladas * 0,75
penalidadeDesconex. = desconexoes        * 1,0

denominador = 1
            + penalidadePortas + penalidadeConexoes + penalidadeTempo
            + penalidadeFalhas + penalidadeCanceladas + penalidadeDesconex.

pontuacao = 1000 / denominador
```

### 2.3 Comportamento

- Partida perfeita (dentro dos valores ideais e sem erros) → **1000 pontos**.
- Portas, conexões e tempo só penalizam o **excesso acima do ideal**. Fazer
  melhor que o ideal não dá bônus, apenas mantém o teto de 1000.
- Falhas, cancelamentos e desconexões penalizam **desde a primeira ocorrência**.
- A pontuação cai de forma acentuada nos primeiros erros e nunca fica negativa.
  O erro mais caro é uma **conexão com falha** (peso `2,0`).

---

## 3. Catálogo de Circuitos

Notação das expressões (precedência da menor para a maior):
`+` = **OU**, `*` = **E**, `!` = **NÃO**, `( )` = agrupamento.
As letras (`A`, `B`, `C`) são as entradas controladas pelo jogador.

### 3.1 Parâmetros ideais por dificuldade

| Dificuldade | Variáveis | Portas ideais | Conexões ideais | Tempo ideal (s) |
|---|---|---|---|---|
| Fácil | A, B | 2 | 4 | 20 |
| Médio | A, B | 3 | 6 | 30 |
| Difícil (1–3) | A, B | 5 | 9 | 40 |
| Difícil (4) | A, B, C | 5 | 12 | 60 |

### 3.2 Fácil

| Circuito | Expressão | Nome comum | Portas | Conexões | Tempo |
|---|---|---|---|---|---|
| EasyCircuit1 | `!(A+B)` | NOR | 2 | 4 | 20 |
| EasyCircuit2 | `!(A*B)` | NAND | 2 | 4 | 20 |
| EasyCircuit3 | `A*!B` | A E NÃO B | 2 | 4 | 20 |
| EasyCircuit4 | `!A+B` | NÃO A OU B (implicação A→B) | 2 | 4 | 20 |

### 3.3 Médio

| Circuito | Expressão | Simplifica para | Portas | Conexões | Tempo |
|---|---|---|---|---|---|
| MediumCircuit1 | `(!A+B)*A` | `A*B` | 3 | 6 | 30 |
| MediumCircuit2 | `(A+!B)*B` | `A*B` | 3 | 6 | 30 |
| MediumCircuit3 | `(A+B)*!A` | `!A*B` | 3 | 6 | 30 |
| MediumCircuit4 | `(A+B)*!B` | `A*!B` | 3 | 6 | 30 |

Os circuitos do nível Médio usam formas redundantes de propósito (a expressão
escrita é mais complexa que a forma mínima), por isso os valores ideais de
portas e conexões consideram a montagem **não simplificada**.

### 3.4 Difícil

| Circuito | Expressão | Nome comum | Variáveis | Portas | Conexões | Tempo |
|---|---|---|---|---|---|---|
| HardCircuit1 | `(A*B)+(!A*!B)` | XNOR (A ≡ B) | A, B | 5 | 9 | 40 |
| HardCircuit2 | `(A+!B)*(!A+B)` | XNOR (A ≡ B) | A, B | 5 | 9 | 40 |
| HardCircuit3 | `!A*B+A*!B` | XOR (A ⊕ B) | A, B | 5 | 9 | 40 |
| HardCircuit4 | `(A*B)+(B*C)+(A*C)` | Maioria de 3 | A, B, C | 5 | 12 | 60 |

`HardCircuit1` e `HardCircuit2` são duas montagens diferentes da mesma função
(XNOR); `HardCircuit3` é a função oposta (XOR).