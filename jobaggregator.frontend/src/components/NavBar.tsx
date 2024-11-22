import React, { useContext, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  IconButton,
  Typography,
  MenuItem,
  Menu,
  Container,
  Box,
  Button,
  Avatar,
  ListItemIcon,
  Divider,
  Drawer,
  List,
  ListItemText,
  ListItemButton,
} from '@mui/material';
import {
  Menu as MenuIcon,
  AccountCircle,
  Logout,
  Person,
  Login,
  Key,
} from '@mui/icons-material';
import { AuthContext } from '../services/AuthContext';
import { useTheme, useMediaQuery } from '@mui/material';

const NavBar: React.FC = () => {
  const authContext = useContext(AuthContext);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [anchorElProfile, setAnchorElProfile] = useState<null | HTMLElement>(null);

  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  if (!authContext) {
    return null;
  }

  const { isAuthenticated, user, logout } = authContext;

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleOpenProfileMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElProfile(event.currentTarget);
  };

  const handleCloseProfileMenu = () => {
    setAnchorElProfile(null);
  };

  const menuItems = [
    { label: 'Domů', path: '/' },
    { label: 'Vyhledávání', path: '/search' },
  ];

  const drawer = (
    <Box
      onClick={handleDrawerToggle}
      sx={{ width: '100%', textAlign: 'center', bgcolor: 'primary.main', color: 'white' }}
    >
      <Typography variant="h6" sx={{ my: 2 }}>
        HledačPráce
      </Typography>
      <Divider />
      <List>
        {menuItems.map((item) => (
          <ListItemButton
            component={Link}
            to={item.path}
            key={item.label}
            sx={{ justifyContent: 'center' }}
          >
            <ListItemText primary={item.label} />
          </ListItemButton>
        ))}
        <Divider />
        {isAuthenticated ? (
          <>
            <ListItemButton
              component={Link}
              to="/profile"
              sx={{ justifyContent: 'center' }}
            >
              <ListItemIcon sx={{ color: 'white', minWidth: '40px' }}>
                <Person />
              </ListItemIcon>
              <ListItemText primary="Profil" />
            </ListItemButton>
            <ListItemButton
              onClick={() => {
                logout();
              }}
              sx={{ justifyContent: 'center' }}
            >
              <ListItemIcon sx={{ color: 'white', minWidth: '40px' }}>
                <Logout />
              </ListItemIcon>
              <ListItemText primary="Odhlásit" />
            </ListItemButton>
          </>
        ) : (
          <>
            <ListItemButton
              component={Link}
              to="/login"
              sx={{ justifyContent: 'center' }}
            >
              <ListItemIcon sx={{ color: 'white', minWidth: '40px' }}>
                <Login />
              </ListItemIcon>
              <ListItemText primary="Přihlášení" />
            </ListItemButton>
            <ListItemButton
              component={Link}
              to="/register"
              sx={{ justifyContent: 'center' }}
            >
              <ListItemIcon sx={{ color: 'white', minWidth: '40px' }}>
                <Key />
              </ListItemIcon>
              <ListItemText primary="Registrace" />
            </ListItemButton>
          </>
        )}
      </List>
    </Box>
  );

  return (
    <AppBar position="sticky" color="primary" elevation={0}>
      <Container maxWidth="lg">
        <Toolbar disableGutters sx={{ justifyContent: 'space-between' }}>
          <Typography
            variant="h6"
            component={Link}
            to="/"
            sx={{
              textDecoration: 'none',
              color: 'inherit',
              display: 'flex',
              alignItems: 'center',
            }}
          >
            HledačPráce
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            {/* Mobile View */}
            {isMobile ? (
              <>
                <IconButton
                  color="inherit"
                  aria-label="open drawer"
                  edge="end"
                  onClick={handleDrawerToggle}
                >
                  <MenuIcon />
                </IconButton>
                <Drawer
                  anchor="top"
                  open={mobileOpen}
                  onClose={handleDrawerToggle}
                  ModalProps={{
                    keepMounted: true,
                  }}
                  PaperProps={{
                    sx: { bgcolor: 'primary.main', color: 'white' },
                  }}
                >
                  {drawer}
                </Drawer>
              </>
            ) : (
              // Desktop View
              <>
                {menuItems.map((item) => (
                  <Button
                    key={item.label}
                    component={Link}
                    to={item.path}
                    sx={{ color: 'white', ml: 2 }}
                  >
                    {item.label}
                  </Button>
                ))}
                {isAuthenticated ? (
                  <>
                    <Button
                      onClick={handleOpenProfileMenu}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        padding: '6px 12px',
                        borderRadius: '24px',
                        backgroundColor: 'rgba(255, 255, 255, 0.1)',
                        '&:hover': {
                          backgroundColor: 'rgba(255, 255, 255, 0.2)',
                        },
                      }}
                    >
                      <Avatar sx={{ bgcolor: 'secondary.main', width: 32, height: 32 }}>
                        <AccountCircle fontSize="small" />
                      </Avatar>
                      <Typography
                        variant="body1"
                        sx={{
                          color: 'white',
                          marginLeft: 1,
                          textTransform: 'capitalize',
                          fontWeight: '500',
                        }}
                      >
                        {user?.username}
                      </Typography>
                    </Button>
                    <Menu
                      id="profile-menu"
                      anchorEl={anchorElProfile}
                      anchorOrigin={{
                        vertical: 'bottom',
                        horizontal: 'right',
                      }}
                      transformOrigin={{
                        vertical: 'top',
                        horizontal: 'right',
                      }}
                      open={Boolean(anchorElProfile)}
                      onClose={handleCloseProfileMenu}
                      PaperProps={{
                        elevation: 3,
                        style: {
                          minWidth: '150px',
                          borderRadius: '8px',
                        },
                      }}
                    >
                      <MenuItem
                        component={Link}
                        to="/profile"
                        onClick={handleCloseProfileMenu}
                      >
                        <ListItemIcon>
                          <Person fontSize="small" />
                        </ListItemIcon>
                        Profil
                      </MenuItem>
                      <MenuItem
                        onClick={() => {
                          logout();
                          handleCloseProfileMenu();
                        }}
                      >
                        <ListItemIcon>
                          <Logout fontSize="small" />
                        </ListItemIcon>
                        Odhlásit
                      </MenuItem>
                    </Menu>
                  </>
                ) : (
                  <>
                    <IconButton
                      onClick={handleOpenProfileMenu}
                      sx={{ p: 0, ml: 2 }}
                    >
                      <Avatar sx={{ bgcolor: 'secondary.main' }}>
                        <AccountCircle />
                      </Avatar>
                    </IconButton>
                    <Menu
                      id="profile-menu"
                      anchorEl={anchorElProfile}
                      anchorOrigin={{
                        vertical: 'bottom',
                        horizontal: 'right',
                      }}
                      transformOrigin={{
                        vertical: 'top',
                        horizontal: 'right',
                      }}
                      open={Boolean(anchorElProfile)}
                      onClose={handleCloseProfileMenu}
                      PaperProps={{
                        elevation: 3,
                        style: {
                          minWidth: '150px',
                          borderRadius: '8px',
                        },
                      }}
                    >
                      <MenuItem
                        component={Link}
                        to="/login"
                        onClick={handleCloseProfileMenu}
                      >
                        <ListItemIcon>
                          <Login fontSize="small" />
                        </ListItemIcon>
                        Přihlášení
                      </MenuItem>
                      <MenuItem
                        component={Link}
                        to="/register"
                        onClick={handleCloseProfileMenu}
                      >
                        <ListItemIcon>
                          <Key fontSize="small" />
                        </ListItemIcon>
                        Registrace
                      </MenuItem>
                    </Menu>
                  </>
                )}
              </>
            )}
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};

export default NavBar;
