import { motion } from "framer-motion";
import logo from "@/assets/logo.png";

interface AnimatedLogoProps {
  readonly className?: string;
  readonly style?: React.CSSProperties;
  readonly size?: "small" | "large";
}

export default function AnimatedLogo({ className, style, size = "small" }: AnimatedLogoProps) {
  const isSmall = size === "small";
  
  return (
    <motion.img
      src={logo}
      alt="HealthSync"
      className={className}
      style={{ 
        height: isSmall ? '24px' : undefined, 
        marginTop: isSmall ? '4px' : undefined,
        width: isSmall ? undefined : '10rem', // w-40 equivalent
        ...style 
      }}
      animate={isSmall ? {
        scale: [1, 1.1, 1],
        rotate: [0, 5, -5, 0]
      } : { 
        rotate: [0, -5, 5, 0] 
      }}
      transition={{
        duration: isSmall ? 2 : 3,
        repeat: Infinity,
        ease: "easeInOut"
      }}
    />
  );
}
