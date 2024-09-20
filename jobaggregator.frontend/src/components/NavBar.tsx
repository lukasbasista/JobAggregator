import React from "react";
import { Link } from "react-router-dom";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import { Button, Container, useScrollTrigger, Slide } from "@mui/material";

function HideOnScroll(props: { children: React.ReactElement }) {
  const { children } = props;
  const trigger = useScrollTrigger();

  return (
    <Slide appear={false} direction="down" in={!trigger}>
      {children}
    </Slide>
  );
}

const NavBar: React.FC = () => {
  return (
    <HideOnScroll>
      <AppBar position="sticky" color="primary" elevation={0}>
        <Container>
          <Toolbar disableGutters>
            <Typography
              variant="h6"
              component={Link}
              to="/"
              sx={{ flexGrow: 1, textDecoration: "none", color: "inherit" }}
            >
              HledačPráce
            </Typography>
            <Button color="inherit" component={Link} to="/">
              Domů
            </Button>
            <Button color="inherit" component={Link} to="/search">
              Vyhledávání
            </Button>
          </Toolbar>
        </Container>
      </AppBar>
    </HideOnScroll>
  );
};

export default NavBar;
