import React from "react";
import { Box, Typography, Container, Link } from "@mui/material";

const Footer: React.FC = () => {
  return (
    <Box sx={{ bgcolor: "primary.main", color: "white", p: 4, mt: "auto" }}>
      <Container maxWidth="lg">
        <Typography variant="body1" align="center">
          © {new Date().getFullYear()} HledačPráce. Všechna práva vyhrazena.
        </Typography>
        <Typography variant="body2" align="center">
          <Link href="#" color="inherit" underline="always">
            O nás
          </Link>{" "}
          |{" "}
          <Link href="#" color="inherit" underline="always">
            Kontakt
          </Link>{" "}
          |{" "}
          <Link href="#" color="inherit" underline="always">
            Podmínky používání
          </Link>
        </Typography>
      </Container>
    </Box>
  );
};

export default Footer;
