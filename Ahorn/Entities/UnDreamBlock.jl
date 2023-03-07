module FlavorsHelperUnDreamBlock

using ..Ahorn, Maple

@mapdef Entity "FlavorsHelper/UnDreamBlock" UnDreamBlock(x::Integer, y::Integer, width::Integer=16, height::Integer=16, activeBackColor::Bool=false)

const colors = sort(collect(keys(Ahorn.XNAColors.colors)))

const placements = Ahorn.PlacementDict(
    "UnDream Block (Flavors Helper)" => Ahorn.EntityPlacement(
        UnDreamBlock,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::UnDreamBlock) = Dict{String, Any}(
)


Ahorn.minimumSize(entity::unDreamBlock) = 8, 8
Ahorn.resizable(entity::UnDreamBlock) = true, true

Ahorn.selection(entity::UnDreamBlock) = Ahorn.getEntityRectangle(entity)

function renderSpaceJam(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)

    Ahorn.drawRectangle(ctx, x, y, width, height, (0.0, 0.0, 0.0, 0.4), (0.5, 0.5, 0.5, 0.5))

    Ahorn.restore(ctx)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::UnDreamBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderSpaceJam(ctx, 0, 0, width, height)
end

end