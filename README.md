
# 🎮 Sistema de Efeitos de Texto com Escrita Letra por Letra (Unity + TextMeshPro)

Este repositório contém um sistema completo para criar **efeitos visuais personalizados em textos com TextMeshPro**, com suporte a **escrita animada**, efeitos dinâmicos em `<tags>` e múltiplas animações combináveis.

---

## ✨ Funcionalidades

✅ Escrita letra por letra com efeitos visuais  
✅ Suporte a múltiplos efeitos simultâneos e aninhados (`<shake><bounce>Texto</bounce></shake>`)  
✅ Tags customizadas com parâmetros (`<wiggle s=10 f=15>`)  
✅ Sistema extensível e desacoplado em duas classes principais (`Writter` e `TextEffectsHandler`)

---

## 🧩 Componentes

### `TextEffectsHandler`

Aplica efeitos dinâmicos em regiões marcadas com `<tags>` no texto do TMP.

#### ✅ Tags disponíveis

| Tag         | Descrição                                        |
|-------------|--------------------------------------------------|
| `<wiggle>`  | Move para cima e baixo em looping                |
| `<shake>`   | Vibração randômica lateral e vertical            |
| `<bounce>`  | Pulo suave contínuo                              |
| `<glitch>`  | Tremor com distorções aleatórias                 |
| `<scale>`   | Pulsa em tamanho                                 |
| `<squash>`  | Distorce largura/altura                          |
| `<fadewave>`| Alterna visibilidade suavemente como uma onda   |
| `<wave>`    | Movimento de serpente vertical                   |
| `<flip>`    | Rotação no eixo Z                                |
| `<explode>` | Letras se afastam do centro temporariamente      |

#### 🔧 Parâmetros nas tags
- `s=` → velocidade (`speed`)
- `f=` → força (`force`)
- Exemplo: `<shake s=10 f=30>tremendo</shake>`

#### 🧠 Combinando efeitos
Você pode aninhar tags e os efeitos se somam, respeitando a ordem:

```text
<shake><bounce>Combinação legal</bounce></shake>
```

---

### `Writter`

Componente que escreve o texto letra por letra com efeitos animados. Após a escrita, os efeitos de `<tags>` são ativados.

#### 📌 Como usar no código

```csharp
public Writter writter;

void Start()
{
    string texto = "<glitch f=15 s=5><scale>Isso é <wave f=10>poderoso</wave></scale></glitch>";
    writter.Escrever(texto, WriteEffectType.Pop);
}
```

#### ✏️ Efeitos de escrita (`WriteEffectType`)

| Tipo               | Efeito durante escrita                            |
|--------------------|---------------------------------------------------|
| `None`             | Sem efeito                                        |
| `FadeIn`           | Alpha de 0 a 1                                    |
| `SlideIn`          | Letra desliza da esquerda                         |
| `ScaleUp`          | Letra cresce do pequeno                           |
| `ScaleDown`        | Letra encolhe do grande                           |
| `TypewriterCursor` | Simula cursor "_" piscando                        |
| `Wave`             | Letra entra em forma de onda                      |
| `Pop`              | Letra “salta” com efeito elástico                |
| `DropIn`           | Letra cai com quique                              |
| `GlitchIn`         | Letra aparece distorcida                          |
| `BlinkOn`          | Letra pisca antes de aparecer                     |

---

## 🛠️ Exemplo na cena

1. Crie um GameObject com `TextMeshProUGUI`
2. Adicione os componentes:
   - `TextEffectsHandler`
   - `Writter`
3. No script, chame:

```csharp
writter.Escrever("<wiggle>Testando</wiggle>", WriteEffectType.ScaleUp);
```

---

## 🗂️ Organização recomendada

| GameObject     | Componentes                                                  |
|----------------|--------------------------------------------------------------|
| `TextObject`   | `TextMeshProUGUI`, `TextEffectsHandler`, `Writter`           |

---

## 📌 Requisitos

- Unity 2021 ou superior  
- TextMeshPro (incluso por padrão no Unity UI Toolkit)

---

## 📄 Licença

MIT. Use livremente em projetos comerciais ou pessoais.  
Créditos são sempre bem-vindos. 😊

---

## ✍️ Autor

Desenvolvido por **Wali Rodrigues**.
Contribuições, issues e sugestões são bem-vindas!
