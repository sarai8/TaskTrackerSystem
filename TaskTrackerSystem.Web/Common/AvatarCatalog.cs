namespace TaskTrackerSystem.Web.Common;

public record AvatarOption(string Id, string Icon, string Color);

public static class AvatarCatalog
{
    public static readonly List<AvatarOption> Options = new()
    {
        new("avatar-1", "bi-emoji-smile", "#0d6efd"),
        new("avatar-2", "bi-emoji-sunglasses", "#6f42c1"),
        new("avatar-3", "bi-robot", "#198754"),
        new("avatar-4", "bi-star-fill", "#fd7e14"),
        new("avatar-5", "bi-lightning-fill", "#ffc107"),
        new("avatar-6", "bi-flower1", "#d63384"),
        new("avatar-7", "bi-moon-stars-fill", "#20c997"),
        new("avatar-8", "bi-cup-hot-fill", "#795548"),
        new("avatar-9", "bi-controller", "#0dcaf0"),
        new("avatar-10", "bi-palette-fill", "#e83e8c"),
        new("avatar-11", "bi-rocket-takeoff-fill", "#6610f2"),
        new("avatar-12", "bi-heart-fill", "#dc3545"),
    };

    public static AvatarOption GetOrDefault(string? id)
        => Options.FirstOrDefault(x => x.Id == id) ?? Options[0];
}