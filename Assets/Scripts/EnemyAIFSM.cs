// ──────────────────────────────────────────────────────────────
// 1) 상태 기본 클래스(상속의 부모)
// ──────────────────────────────────────────────────────────────
using UnityEngine;

public abstract class EnemyAIState
{
    // 상태 진입/이탈 훅(필요 없으면 비워둬도 됨)
    public virtual void Enter(EnemyController_OO ctx) { }
    public virtual void Exit(EnemyController_OO ctx) { }

    // 매 프레임 호출. 상태를 유지하면 null, 바꾸려면 새 상태 인스턴스를 반환.
    public abstract EnemyAIState Update(EnemyController_OO ctx);
}

// ──────────────────────────────────────────────────────────────
// 3) 구체 상태들(Idle / Chase / Attack)
// ──────────────────────────────────────────────────────────────
public class IdleState : EnemyAIState
{
    public override EnemyAIState Update(EnemyController_OO ctx)
    {
        float dist = ctx.DistanceToPlayer();

        // 대기: 너무 멀면 멈춰있기
        if (dist > ctx.detectRange)
        {
            ctx.agent.isStopped = true;
            return null; // 상태 유지
        }

        // 감지 범위 안: 추적으로 전환
        return new ChaseState();
    }
}

public class ChaseState : EnemyAIState
{
    public override EnemyAIState Update(EnemyController_OO ctx)
    {
        float dist = ctx.DistanceToPlayer();

        // 감지 밖: Idle
        if (dist > ctx.detectRange)
        {
            ctx.agent.isStopped = true;
            return new IdleState();
        }

        // 공격 조건 충족 시 Attack
        bool canAttack = false;
        if (ctx.attackMode == EnemyController_OO.AttackAIMode.MeleeOnly)
            canAttack = (dist <= ctx.meleeRange);
        else if (ctx.attackMode == EnemyController_OO.AttackAIMode.RangedOnly)
            canAttack = (dist <= ctx.rangedRange);
        else // Hybrid
            canAttack = (dist <= ctx.rangedRange);

        if (canAttack)
        {
            ctx.agent.isStopped = true;
            return new AttackState();
        }

        // 추적 계속
        ctx.agent.isStopped = false;
        ctx.agent.SetDestination(ctx.player.position);
        return null; // 유지
    }
}

public class AttackState : EnemyAIState
{
    // 공격 전략(다형성) 보관
    MeleeAttackStrategy melee = new MeleeAttackStrategy();
    RangedAttackStrategy ranged = new RangedAttackStrategy();

    public override EnemyAIState Update(EnemyController_OO ctx)
    {
        float dist = ctx.DistanceToPlayer();

        // 감지 밖 → Idle
        if (dist > ctx.detectRange)
        {
            ctx.agent.isStopped = true;
            return new IdleState();
        }

        // 공격 범위 밖 → Chase
        bool insideAttackArea = false;
        if (ctx.attackMode == EnemyController_OO.AttackAIMode.MeleeOnly)
            insideAttackArea = (dist <= ctx.meleeRange);
        else if (ctx.attackMode == EnemyController_OO.AttackAIMode.RangedOnly)
            insideAttackArea = (dist <= ctx.rangedRange);
        else
            insideAttackArea = (dist <= ctx.rangedRange);

        if (!insideAttackArea)
        {
            return new ChaseState();
        }

        // 공격 수행
        ctx.agent.isStopped = true;
        ctx.FacePlayerHorizontally();

        if (ctx.attackMode == EnemyController_OO.AttackAIMode.MeleeOnly)
        {
            melee.TryAttack(ctx, dist);
        }
        else if (ctx.attackMode == EnemyController_OO.AttackAIMode.RangedOnly)
        {
            ranged.TryAttack(ctx, dist);
        }
        else // Hybrid: 거리로 전략 선택(히스테리시스)
        {
            // 이미 근접 중이면 좀 더 멀어질 때까지 근접 유지
            if (ctx.usingMelee)
            {
                if (dist > ctx.meleeRange + ctx.meleeExitBuffer)
                    ctx.usingMelee = false;
            }
            else
            {
                if (dist <= ctx.meleeRange)
                    ctx.usingMelee = true;
            }

            if (ctx.usingMelee) melee.TryAttack(ctx, dist);
            else ranged.TryAttack(ctx, dist);
        }

        return null; // 공격 상태 유지
    }
}

// ──────────────────────────────────────────────────────────────
// 4) 공격 전략(상속/다형성) - 인터페이스 대신 추상 부모 + 자식 2개
// ──────────────────────────────────────────────────────────────
public abstract class AttackStrategy
{
    public abstract void TryAttack(EnemyController_OO ctx, float distToPlayer);
}

public class MeleeAttackStrategy : AttackStrategy
{
    public override void TryAttack(EnemyController_OO ctx, float distToPlayer)
    {
        // 쿨타임
        if (Time.time < ctx.nextMeleeTime) return;

        // 내 앞에 작은 원으로 판정
        Vector3 center = ctx.transform.position + ctx.transform.forward * (ctx.meleeRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, ctx.hitRadius, ctx.playerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerStatus ps = hits[i].GetComponent<PlayerStatus>();
            if (ps != null)
            {
                ps.TakeDamage(ctx.meleeDamage); // 실제 데미지
                break; // 한 번만
            }
        }
        ctx.nextMeleeTime = Time.time + ctx.meleeCooldown;
        // (선택) 근접 애니/사운드 트리거 가능
    }
}

public class RangedAttackStrategy : AttackStrategy
{
    public override void TryAttack(EnemyController_OO ctx, float distToPlayer)
    {
        // 쿨타임
        if (Time.time < ctx.nextRangedTime) return;
        if (ctx.projectilePrefab == null || ctx.firePoint == null) return;

        GameObject proj = GameObject.Instantiate(ctx.projectilePrefab, ctx.firePoint.position, ctx.firePoint.rotation);

        // Projectile 값 덮어쓰기(공개 필드 가정)
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.speed = ctx.projectileSpeed;
            p.damage = ctx.projectileDamage;
        }

        ctx.nextRangedTime = Time.time + ctx.rangedCooldown;
        // (선택) 머즐 플래시/사운드 가능
    }
}