-- -----------------------------------------------------------------------------
MERGE dbo.Categoria AS target
USING (VALUES
    ('Entrada'),
    ('Plato Fuerte'),
    ('Postre'),
    ('Bebidas'),
    ('Acompañamiento')
) AS source (Nombre)
ON target.Nombre = source.Nombre
WHEN NOT MATCHED THEN
    INSERT (Nombre, Activo) VALUES (source.Nombre, 1)
WHEN MATCHED THEN
    UPDATE SET target.Activo = 1;

PRINT 'Categorías sincronizadas.';
GO

-- Desactivar categorías antiguas
UPDATE dbo.Categoria SET Activo = 0 
WHERE Nombre IN ('Frutas', 'Verduras', 'Insumos', 'Líquidos', 'Carnes', 'Otros');

PRINT 'Categorías antiguas desactivadas.';
GO

-- -----------------------------------------------------------------------------
-- CLIENTES (idempotente por Cedula)
-- -----------------------------------------------------------------------------
INSERT INTO dbo.Cliente (Nombre, Cedula, Correo, Telefono, Direccion, UsuarioIdentityId)
SELECT Nombre, Cedula, Correo, Telefono, Direccion, NULL
FROM (VALUES
    ('Ana Sofía Rodríguez Mora',        '101230001', 'ana.rodriguez@gmail.com',     '88001001', 'San José, Escazú, 100m norte del parque'),
    ('Carlos Andrés Jiménez Solís',     '201450002', 'carlos.jimenez@hotmail.com',  '88002002', 'Alajuela, Centro, Av. 3'),
    ('María Fernanda Castro López',     '301670003', 'mf.castro@outlook.com',       '88003003', 'Cartago, Tres Ríos, residencial Las Palmas'),
    ('Luis Diego Pérez Vargas',         '401890004', 'luis.perez@yahoo.com',        '88004004', 'Heredia, San Pablo, calle principal'),
    ('Valeria Alejandra Mora Chaves',   '501110005', 'valeria.mora@gmail.com',      '88005005', 'Liberia, Guanacaste, frente al estadio'),
    ('Andrés Felipe Núñez Segura',      '601330006', 'andres.nunez@gmail.com',      '88006006', 'Puntarenas, Centro, edificio Costa Rica'),
    ('Daniela Paola Ulate Quirós',      '701550007', 'daniela.ulate@hotmail.com',   '88007007', 'San José, Desamparados, barrio El Carmen'),
    ('Roberto Carlos Brenes Araya',     '801770008', 'roberto.brenes@gmail.com',    '88008008', 'Alajuela, Atenas, frente a la iglesia'),
    ('Paola Vanessa Solano Vega',       '109990009', 'paola.solano@outlook.com',    '88009009', 'Cartago, Paraíso, barrio San Miguel'),
    ('Óscar Mauricio Herrera Fonseca',  '210210010', 'oscar.herrera@yahoo.com',     '88010010', 'Heredia, Barva, residencial Los Robles'),
    ('Gabriela Marcela Torres Blanco',  '311430011', 'gabriela.torres@gmail.com',   '88011011', 'San José, La Uruca, zona industrial'),
    ('Juan Pablo Calvo Elizondo',       '412650012', 'jp.calvo@hotmail.com',        '88012012', 'Alajuela, San Ramón, calle Real'),
    ('Stephanie Vanessa Rojas Arias',   '513870013', 'stephanie.rojas@gmail.com',   '88013013', 'Limón, Puerto Viejo, frente al mar'),
    ('Diego Alejandro Mata Gamboa',     '614090014', 'diego.mata@outlook.com',      '88014014', 'San José, Tibás, avenida principal'),
    ('Karla Patricia Sánchez Monge',    '715310015', 'karla.sanchez@yahoo.com',     '88015015', 'Heredia, Flores, urbanización Los Arcos'),
    ('Fernando José Alvarado Cruz',     '816530016', 'fernando.alvarado@gmail.com', '88016016', 'Cartago, La Unión, barrio Trinidad'),
    ('Natalia Cristina Espinoza Ruiz',  '117750017', 'natalia.espinoza@hotmail.com','88017017', 'Guanacaste, Santa Cruz, residencial Tamarindo'),
    ('Marco Antonio Fallas Ureña',      '218970018', 'marco.fallas@gmail.com',      '88018018', 'Alajuela, Grecia, frente al Mercado Municipal'),
    ('Priscilla Sofía Vindas Mora',     '319190019', 'priscilla.vindas@outlook.com','88019019', 'San José, Moravia, calle San Blas'),
    ('Esteban Rodrigo Quesada Salaz',   '420410020', 'esteban.quesada@yahoo.com',   '88020020', 'Heredia, Santo Domingo, zona comercial'),
    ('Alejandra Isabel Badilla León',   '521630021', 'alejandra.badilla@gmail.com', '88021021', 'Cartago, Oreamuno, barrio San Pedro'),
    ('Randall Esteban Vega Chacón',     '622850022', 'randall.vega@hotmail.com',    '88022022', 'Puntarenas, Quepos, frente al Puerto'),
    ('Melissa Johanna Picado Mora',     '723070023', 'melissa.picado@gmail.com',    '88023023', 'San José, Hatillo, urbanización Sol Naciente'),
    ('Bryan Josué Arce Herrera',        '824290024', 'bryan.arce@outlook.com',      '88024024', 'Alajuela, Palmares, barrio El Estudio'),
    ('Silvia Patricia Cubero Campos',   '925510025', 'silvia.cubero@yahoo.com',     '88025025', 'Heredia, San Isidro, residencial El Prado')
) AS v (Nombre, Cedula, Correo, Telefono, Direccion)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Cliente c WHERE c.Cedula = v.Cedula
);

PRINT 'Clientes nuevos insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- -----------------------------------------------------------------------------
-- PRODUCTOS (idempotente por Nombre)
-- -----------------------------------------------------------------------------
DECLARE @imgBase NVARCHAR(300) = '/imagenes/productos/caribbean-logo.png';

DECLARE @idEntrada         INT = (SELECT Id FROM dbo.Categoria WHERE Nombre = 'Entrada' AND Activo = 1);
DECLARE @idPlatoFuerte     INT = (SELECT Id FROM dbo.Categoria WHERE Nombre = 'Plato Fuerte' AND Activo = 1);
DECLARE @idPostre          INT = (SELECT Id FROM dbo.Categoria WHERE Nombre = 'Postre' AND Activo = 1);
DECLARE @idBebidas         INT = (SELECT Id FROM dbo.Categoria WHERE Nombre = 'Bebidas' AND Activo = 1);
DECLARE @idAcompañamiento  INT = (SELECT Id FROM dbo.Categoria WHERE Nombre = 'Acompañamiento' AND Activo = 1);

IF @idEntrada IS NULL OR @idPlatoFuerte IS NULL OR @idPostre IS NULL
   OR @idBebidas IS NULL OR @idAcompañamiento IS NULL
BEGIN
    PRINT 'ERROR: Falta al menos una categoría activa. Ejecuta primero el bloque de Categorías.';
END
ELSE
BEGIN
    INSERT INTO dbo.Producto (Nombre, CategoriaId, Precio, ImpuestoPorc, Stock, ImagenUrl, Activo)
    SELECT Nombre, CategoriaId, Precio, ImpuestoPorc, Stock, @imgBase, 1
    FROM (VALUES
        -- Entrada (3)
        ('Ceviche Caribeño',        @idEntrada,       6500.00,  13.00, 40),
        ('Patties de Carne',        @idEntrada,       3500.00,  13.00, 80),
        ('Nachos con Guacamole',    @idEntrada,       5000.00,  13.00, 60),
        
        -- Plato Fuerte (4)
        ('Rice and Beans',          @idPlatoFuerte,   7500.00,  13.00, 100),
        ('Ropa Vieja',              @idPlatoFuerte,   9000.00,  13.00, 50),
        ('Mofongo con Camarones',   @idPlatoFuerte,  10500.00,  13.00, 45),
        ('Pescado a la Sal',        @idPlatoFuerte,  11000.00,  13.00, 35),
        
        -- Postre (3)
        ('Flan de Coco',            @idPostre,        3500.00,  13.00, 60),
        ('Brazo Gitano',            @idPostre,        4000.00,  13.00, 50),
        ('Tarta de Chocolate',      @idPostre,        4500.00,  13.00, 40),
        
        -- Bebidas (3)
        ('Agua de Jamaica',         @idBebidas,       2500.00,  13.00, 200),
        ('Ron con Cola',            @idBebidas,       5000.00,  13.00, 120),
        ('Jugo de Coco Natural',    @idBebidas,       3500.00,  13.00, 80),
        
        -- Acompañamiento (2)
        ('Tostones',                @idAcompañamiento, 2500.00, 13.00, 100),
        ('Yuca Frita',              @idAcompañamiento, 2800.00, 13.00, 90)
    ) AS v (Nombre, CategoriaId, Precio, ImpuestoPorc, Stock)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Producto p WHERE p.Nombre = v.Nombre
    );

    PRINT 'Productos nuevos insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
END
GO
