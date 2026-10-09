using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    [SerializeField] private GameObject EnemySprite;
    [SerializeField] protected float detectionRange = 3;
    [SerializeField] protected float fireCharge = 1f;

    protected PlayerController playerController;
    
    protected Task fireTask;

    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private Vector3 lifebarOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private Vector2 lifebarSize = new Vector2(1f, 0.12f);

    //public static readonly List<EnemyController> Active = new List<EnemyController>();

    [Header("Boss")]
    [SerializeField] protected bool isBoss;
    [SerializeField] private float bossScale = 5f;
    [SerializeField] private float bossHealthMultiplier = 4f;
    [SerializeField] private float bossProjectileScale = 2f;

    protected int health;
    private LevelManager levelManager;
    private Transform lifebarRoot;
    private Transform lifebarFill;
    private SpriteRenderer lifebarFillRenderer;
    private static Sprite whiteSprite;

    // Multiplier for sizes and distances (radii, ranges) so bosses scale with their body.
    protected float SizeFactor => isBoss ? bossScale : 1f;

    private void Awake()
    {
        if (isBoss)
        {
            transform.localScale *= bossScale;
            maxHealth = Mathf.RoundToInt(maxHealth * bossHealthMultiplier);
            detectionRange *= bossScale;
            hitRadius *= bossScale;
            lifebarOffset *= bossScale;
            lifebarSize *= bossScale;
        }

        health = maxHealth;
        CreateLifebar();
    }

    // Enlarges projectiles fired by bosses.
    protected void SpawnProjectile(ProjectileController prefab, Vector3 direction)
    {
        var projectile = Instantiate(prefab);
        projectile.transform.position = transform.position;
        if (isBoss)
        {
            projectile.transform.localScale *= bossProjectileScale;
        }

        projectile.SetProjectileDirection(direction);
    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelReset -= ResetEnemy;
        }

        if (lifebarRoot != null)
        {
            Destroy(lifebarRoot.gameObject);
        }
    }

    public bool IsAlive => health > 0;

    public bool IsBoss => isBoss;

    public static event System.Action<EnemyController> BossDefeated;

    public virtual void TakeDamage(int amount)
    {
        bool wasAlive = health > 0;
        health = Mathf.Max(0, health - amount);
        UpdateLifebar();

        if (health <= 0)
        {
            AvatarDead();
            if (wasAlive && isBoss)
            {
                BossDefeated?.Invoke(this);
            }
        }
    }

    protected static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0, 0.5f), 1f);
        }
        return whiteSprite;
    }

    private void CreateLifebar()
    {
        // Not parented to the enemy so it does not rotate with it.
        lifebarRoot = new GameObject("Lifebar").transform;

        var background = new GameObject("Background").AddComponent<SpriteRenderer>();
        background.transform.SetParent(lifebarRoot, false);
        background.sprite = GetWhiteSprite();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        background.sortingOrder = 100;
        background.transform.localPosition = new Vector3(-lifebarSize.x / 2f, 0, 0);
        background.transform.localScale = new Vector3(lifebarSize.x, lifebarSize.y, 1);

        lifebarFillRenderer = new GameObject("Fill").AddComponent<SpriteRenderer>();
        lifebarFill = lifebarFillRenderer.transform;
        lifebarFill.SetParent(lifebarRoot, false);
        lifebarFillRenderer.sprite = GetWhiteSprite();
        lifebarFillRenderer.sortingOrder = 101;
        lifebarFill.localPosition = new Vector3(-lifebarSize.x / 2f, 0, 0);

        UpdateLifebar();
        LateUpdate();
    }

    private void UpdateLifebar()
    {
        float ratio = maxHealth > 0 ? (float)health / maxHealth : 0f;
        lifebarFill.localScale = new Vector3(lifebarSize.x * ratio, lifebarSize.y, 1);

        // Green (100%) -> yellow (50%) -> red (0%)
        lifebarFillRenderer.color = ratio >= 0.5f
            ? Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f)
            : Color.Lerp(Color.red, Color.yellow, ratio * 2f);
    }

    private void LateUpdate()
    {
        if (lifebarRoot != null)
        {
            lifebarRoot.position = transform.position + lifebarOffset;
            lifebarRoot.rotation = Quaternion.identity;
        }
    }

    private void Start()
    {
        levelManager = ManagerLocator.Get<LevelManager>();
        levelManager.OnLevelReset += ResetEnemy;
        playerController = levelManager.GetPlayerController();
    }

    protected virtual void ResetEnemy(PlayerController player)
    {
        playerController = player;
        health = maxHealth;
        UpdateLifebar();
        EnemySprite.SetActive(true);
    }
    
    protected abstract Task AvatarFire(object value);

    protected virtual void AvatarDead()
    {
        EnemySprite.SetActive(false);
    }

    private void Update()
    {
        if(playerController == null || health <= 0)
        {
            return;
        }

        Behave();
    }

    // Default behaviour: face the player and fire while in range. Subclasses can override.
    protected virtual void Behave()
    {
        if ((playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }
        
        Vector3 direction = playerController.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
        transform.rotation = rotation;
        
        if(fireTask is { IsCompleted: false })
        {
            return;
        }
        
        fireTask = AvatarFire(playerController.transform);
    }

    // Nearest hit that is not a character (enemies and the player are not walls).
    protected static bool TryFindWall(RaycastHit2D[] hits, Vector2 moveDirection, out float distance)
    {
        distance = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponentInParent<EnemyController>() != null ||
                hit.collider.GetComponentInParent<PlayerController>() != null)
            {
                continue;
            }

            // Ignore surfaces we are moving away from or sliding along (e.g. the wall we just stopped at).
            if (Vector2.Dot(hit.normal, moveDirection) >= -0.01f)
            {
                continue;
            }

            distance = Mathf.Min(distance, hit.distance);
        }

        return distance != float.MaxValue;
    }
}
