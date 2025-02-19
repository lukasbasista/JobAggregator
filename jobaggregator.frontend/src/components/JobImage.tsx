import React, { useEffect, useState } from "react";

interface JobImageProps {
  companyLogo?: string;
  portalLogo?: string;
  placeholder?: string;
  alt?: string;
}

const JobImage: React.FC<JobImageProps> = ({
  companyLogo,
  portalLogo,
  placeholder = "https://via.placeholder.com/150",
  alt,
}) => {
  const [imgSrc, setImgSrc] = useState(companyLogo || portalLogo || placeholder);

  useEffect(() => {
    setImgSrc(companyLogo || portalLogo || placeholder);
  }, [companyLogo, portalLogo, placeholder]);

  const handleImageError = () => {
    if (imgSrc !== (portalLogo || placeholder)) {
      setImgSrc(portalLogo || placeholder);
    }
  };

  return (
    <img
      src={imgSrc}
      alt={alt || "Job image"}
      onError={handleImageError}
      style={{
        width: "100%",
        maxHeight: "140px",
        objectFit: "contain",
        objectPosition: "center",
        display: "block",
        backgroundColor: "#f5f5f5"
    }}
    />
  );
};

export default JobImage;
export {}