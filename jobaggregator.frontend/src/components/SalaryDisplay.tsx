import React from "react";
import { Typography } from "@mui/material";

interface SalaryDisplayProps {
  salaryFrom?: number;
  salaryTo?: number;
  currency: string;
}

const SalaryDisplay: React.FC<SalaryDisplayProps> = ({ salaryFrom, salaryTo, currency }) => {
  if ((!salaryFrom || salaryFrom === 0) && salaryTo) {
    return (
      <Typography variant="h6" sx={{ fontWeight: "bold", color: "success.main" }}>
        až {salaryTo} {currency}
      </Typography>
    );
  } else if ((!salaryTo || salaryTo === 0) && salaryFrom) {
    return (
      <Typography variant="h6" sx={{ fontWeight: "bold", color: "success.main" }}>
        Od {salaryFrom} {currency}
      </Typography>
    );
  } else if (salaryFrom && salaryTo) {
    return (
      <Typography variant="h6" sx={{ fontWeight: "bold", color: "success.main" }}>
        {salaryFrom} - {salaryTo} {currency}
      </Typography>
    );
  }
  return null;
};

export default SalaryDisplay;
export {};