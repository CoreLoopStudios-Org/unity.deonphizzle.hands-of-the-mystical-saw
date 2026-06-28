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
    // 🌟 নতুন ডাটা (Stone Market UI এর জন্য)
    // ==========================================
    public string stone_uid;         // পাথরের ইউনিক আইডি (সার্ভার থেকে আসবে)
    public int challenge_points;     // কার্ডে দেখানোর জন্য (যেমন: 85000)
    public int total_weight_kg;      // কার্ডে দেখানোর জন্য (যেমন: 250)
    public int stone_icon_index;     // কোন ছবিটা দেখাবে (0, 1, 2...)
    public string stone_size_label;
    // ==========================================

    // আগের কোর ডাটাগুলো
    public PhysicsAndMaterial physics_and_material;
    public JadeCore jade_core;
    public RotationSystem rotation_system;
    public AnchorNetwork anchor_network;
    public string adversity_level;
    
    // ==========================================
    // 🌟 নতুন ডেটা: Predictor-এর GDD Rules (Phase 1)
    // ==========================================
    public StoneChallengeData predictor_challenge_data;
    
    
}
