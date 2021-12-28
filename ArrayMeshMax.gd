extends Object


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

signal array_mesh_max(array_max)

# Called when the node enters the scene tree for the first time.
func _ready():
	emit_signal("array_mesh_max", ArrayMesh.ARRAY_MAX)


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
