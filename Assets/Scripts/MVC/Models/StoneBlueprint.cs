using System;

[Serializable]
public class PhysicsAndMaterial 
{ 
    public float size_scale; 
    public string density; 
    public string stress; 
    public string fracture_tolerance; 
}

[Serializable]
public class RotationSystem 
{ 
    public float speed; 
    public float rotation_angle; 
    public string rotation_pattern; // LeftToRight or RightToLeft
    public string spin_speed;       // Slow or Fast
}

[Serializable]
public class AnchorNetwork 
{ 
    public string type; 
    public int point_count; 
}

[Serializable]
public class JadeCore 
{ 
    public string color_rating; 
    public int quantity_mass; 
}

[Serializable]
public class StoneBlueprint
{
    // ==========================================
    // 🌟 new data (for Stone Market UI)
    // ==========================================
    public string stone_uid;         //Unique ID of the stone (will come from the server)
    public int challenge_points;     // to show on card (eg: 85000)
    public int total_weight_kg;      // to show on card (eg: 250)
    public int stone_icon_index;     // Which image to display (0, 1, 2...)
    public string stone_size_label;
    // ==========================================

    // Previous core data
    public PhysicsAndMaterial physics_and_material;
    public JadeCore jade_core;
    public RotationSystem rotation_system;
    public AnchorNetwork anchor_network;
    public string adversity_level;
    
    // ==========================================
    // 🌟 New Data: Predictor's GDD Rules (Phase 1)
    // ==========================================
    public StoneChallengeData predictor_challenge_data;
    
    
}
