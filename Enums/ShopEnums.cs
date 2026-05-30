namespace SnakeClaude.Enums;

/// <summary>Categorias principais da loja.</summary>
public enum ShopCategory
{
    Cosmetics    = 0,
    Items        = 1,
    GameModes    = 2,
    Themes       = 3
}

/// <summary>Estado de um item na loja/inventário do jogador.</summary>
public enum ShopItemState
{
    Locked       = 0,   // Não possui, não pode comprar ainda
    Available    = 1,   // Disponível para compra
    Purchased    = 2,   // Comprado mas não equipado
    Equipped     = 3,   // Comprado e equipado/ativo
    Unavailable  = 4    // Indisponível temporariamente (evento, manutenção)
}

/// <summary>Tipo de cosmético.</summary>
public enum CosmeticType
{
    SnakeSkin    = 0,   // Skin da cobra
    FoodEffect   = 1,   // Efeito visual da comida
    TrailEffect  = 2,   // Rastro da cobra
    BoardTheme   = 3    // Tema do tabuleiro
}

/// <summary>Tipo de item consumível/boost.</summary>
public enum ItemType
{
    Continue     = 0,   // Continua após game over
    ScoreBoost   = 1,   // Multiplicador de pontos temporário
    SlowMotion   = 2,   // Reduz velocidade temporariamente
    Shield       = 3,   // Proteção contra colisão
    FoodMagnet   = 4    // Atrai comida
}

/// <summary>Tipo de modo de jogo desbloqueável.</summary>
public enum GameModeType
{
    Infinite     = 0,   // Modo sem fim de mapa
    Hardcore     = 1,   // Morte instantânea, sem pausa
    TimeAttack   = 2,   // Contra o relógio
    Labyrinth    = 3,   // Paredes extras no mapa
    Ghost        = 4    // Atravessa paredes
}

/// <summary>Raridade do item.</summary>
public enum ItemRarity
{
    Common       = 0,
    Rare         = 1,
    Epic         = 2,
    Legendary    = 3
}
